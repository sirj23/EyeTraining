using System;
using System.Collections.Generic;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Progression.Saccades;
using EyeTraining.Sessions.Progression.VisualSearch;
using EyeTraining.Sessions.Progression.Peripheral;
using EyeTraining.Sessions.Rotation;
using EyeTraining.Sessions.Rotation.Returning;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class SessionScheduler
    {
        public const int TargetReturningTrackingCount = 4;

        private static readonly TimeSpan StandardHardLimit = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MilestoneHardLimit = TimeSpan.FromMinutes(20);

        private readonly UnlockService _unlockService;
        private readonly RotationService _rotationService;
        private readonly TrackingProgressionService _progressionService;
        private readonly NumberJourneyProgressionService _numberJourneyProgressionService;
        private readonly ShapeSearchProgressionService _shapeSearchProgressionService;
        private readonly EdgeSignalsProgressionService _edgeSignalsProgressionService;
        private readonly ReturningExerciseSelector _returningExerciseSelector;
        private readonly MajorUnlockPacePolicy _majorUnlockPacePolicy;
        private readonly DiversitySlotCadencePolicy _diversitySlotCadencePolicy;
        private readonly TrackingExerciseCatalog _trackingCatalog;
        private readonly ITrackingDurationEstimator _durationEstimator;
        private readonly ILandoltSchedulePolicy _landoltSchedulePolicy;

        public SessionScheduler(
            UnlockService unlockService,
            RotationService rotationService,
            TrackingProgressionService progressionService,
            NumberJourneyProgressionService numberJourneyProgressionService,
            ShapeSearchProgressionService shapeSearchProgressionService,
            EdgeSignalsProgressionService edgeSignalsProgressionService,
            ReturningExerciseSelector returningExerciseSelector,
            MajorUnlockPacePolicy majorUnlockPacePolicy,
            DiversitySlotCadencePolicy diversitySlotCadencePolicy,
            TrackingExerciseCatalog trackingCatalog,
            ITrackingDurationEstimator durationEstimator,
            ILandoltSchedulePolicy landoltSchedulePolicy)
        {
            _unlockService = unlockService ?? throw new ArgumentNullException(nameof(unlockService));
            _rotationService = rotationService ?? throw new ArgumentNullException(nameof(rotationService));
            _progressionService = progressionService
                ?? throw new ArgumentNullException(nameof(progressionService));
            _numberJourneyProgressionService = numberJourneyProgressionService
                ?? throw new ArgumentNullException(nameof(numberJourneyProgressionService));
            _shapeSearchProgressionService = shapeSearchProgressionService
                ?? throw new ArgumentNullException(nameof(shapeSearchProgressionService));
            _edgeSignalsProgressionService = edgeSignalsProgressionService
                ?? throw new ArgumentNullException(nameof(edgeSignalsProgressionService));
            _returningExerciseSelector = returningExerciseSelector
                ?? throw new ArgumentNullException(nameof(returningExerciseSelector));
            _majorUnlockPacePolicy = majorUnlockPacePolicy
                ?? throw new ArgumentNullException(nameof(majorUnlockPacePolicy));
            _diversitySlotCadencePolicy = diversitySlotCadencePolicy
                ?? throw new ArgumentNullException(nameof(diversitySlotCadencePolicy));
            _majorUnlockPacePolicy.Validate(_unlockService.Plan);
            _trackingCatalog = trackingCatalog
                ?? throw new ArgumentNullException(nameof(trackingCatalog));
            _durationEstimator = durationEstimator
                ?? throw new ArgumentNullException(nameof(durationEstimator));
            _landoltSchedulePolicy = landoltSchedulePolicy
                ?? throw new ArgumentNullException(nameof(landoltSchedulePolicy));
        }

        public SessionScheduleResult Schedule(SessionScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            UnlockState stateBefore = _unlockService.GetState(request.CompletedSessionCount);
            UnlockState stateAtCurrentThreshold = _unlockService.GetState(request.CurrentSessionNumber);
            SessionType sessionType = stateAtCurrentThreshold.IsMilestone
                ? SessionType.Milestone
                : SessionType.Standard;
            TimeSpan hardLimit = sessionType == SessionType.Milestone
                ? MilestoneHardLimit
                : StandardHardLimit;

            List<string> newTrackingIds = SelectTracking(
                stateAtCurrentThreshold.NewlyUnlockedExerciseIds);
            List<string> availableTrackingIds = BuildAvailableTrackingIds(
                stateBefore.UnlockedExerciseIds,
                newTrackingIds);

            var rotationRequest = new RotationRequest(
                request.CurrentSessionNumber,
                availableTrackingIds,
                newTrackingIds,
                request.RotationHistory,
                TargetReturningTrackingCount);
            RotationResult rotationResult = _rotationService.Select(rotationRequest);

            List<ScheduledTracking> returning = BuildTrackingExercises(
                rotationResult.SelectedExerciseIds,
                request);
            List<ScheduledTracking> newlyUnlocked = BuildTrackingExercises(
                newTrackingIds,
                request);
            string newlyUnlockedAdditionalId = SelectNewAdditionalExercise(
                stateAtCurrentThreshold.NewlyUnlockedExerciseIds);
            int knownAdditionalCount = CountKnownAdditionalExercises(stateBefore.UnlockedExerciseIds);
            bool cadenceAllowsSlot = _diversitySlotCadencePolicy.CanUseSlot(
                request.CurrentSessionNumber, knownAdditionalCount, request.ReturningExerciseHistory);
            if (newlyUnlockedAdditionalId != null && !cadenceAllowsSlot)
                throw new InvalidOperationException("A configured major unlock violates diversity slot cadence.");
            ScheduledAdditional additional = newlyUnlockedAdditionalId == null
                ? null
                : BuildAdditional(newlyUnlockedAdditionalId, true, request);
            int? nextAdditionalUnlockSession = FindNextAdditionalUnlockSession(request.CurrentSessionNumber);
            if (additional == null && cadenceAllowsSlot
                && _diversitySlotCadencePolicy.ReturningWouldPreserveNextUnlock(
                    request.CurrentSessionNumber, knownAdditionalCount, nextAdditionalUnlockSession))
            {
                string returningId = _returningExerciseSelector.Select(
                    request.CurrentSessionNumber,
                    stateBefore.UnlockedExerciseIds,
                    request.ReturningExerciseHistory);
                if (returningId != null) additional = BuildAdditional(returningId, false, request);
            }
            bool includeLandolt = _landoltSchedulePolicy.ShouldSchedule(request.CurrentSessionNumber);

            TimeSpan requiredDuration = SessionSchedulingDefinitions.PreparationBasic.EstimatedDuration.Value
                + SumDuration(newlyUnlocked)
                + (additional != null && additional.IsNew ? additional.Duration : TimeSpan.Zero)
                + (includeLandolt
                    ? SessionSchedulingDefinitions.LandoltStandard.EstimatedDuration.Value
                    : TimeSpan.Zero);

            if (requiredDuration > hardLimit)
            {
                return CreateFailureResult(
                    stateAtCurrentThreshold,
                    hardLimit,
                    rotationResult.SelectedExerciseIds.Count);
            }

            bool wasReturningCountReducedForTime = false;
            while (requiredDuration + SumDuration(returning) > hardLimit)
            {
                returning.RemoveAt(returning.Count - 1);
                wasReturningCountReducedForTime = true;
            }

            ScheduledTracking displacedForDiversity = null;
            if (additional != null && returning.Count > 0)
            {
                displacedForDiversity = returning[returning.Count - 1];
                returning.RemoveAt(returning.Count - 1);
            }

            if (additional != null && !additional.IsNew
                && requiredDuration + SumDuration(returning) + additional.Duration > hardLimit)
            {
                additional = null;
                if (displacedForDiversity != null
                    && requiredDuration + SumDuration(returning) + displacedForDiversity.Duration <= hardLimit)
                    returning.Add(displacedForDiversity);
            }

            List<PlannedExercise> exercises = BuildOrderedPlan(
                returning,
                newlyUnlocked,
                additional,
                includeLandolt);
            var plan = new SessionPlan(sessionType, exercises);

            return new SessionScheduleResult(
                SessionScheduleStatus.Success,
                plan,
                stateAtCurrentThreshold.NewlyUnlockedExerciseIds,
                stateAtCurrentThreshold.NewlyUnlockedFamilies,
                hardLimit,
                TargetReturningTrackingCount,
                returning.Count,
                wasReturningCountReducedForTime);
        }

        private List<ScheduledTracking> BuildTrackingExercises(
            IReadOnlyList<string> exerciseIds,
            SessionScheduleRequest request)
        {
            var exercises = new List<ScheduledTracking>(exerciseIds.Count);

            for (var index = 0; index < exerciseIds.Count; index++)
            {
                string exerciseId = exerciseIds[index];
                TrackingProgressionState progression = _progressionService.GetState(
                    exerciseId,
                    request.TrackingProgressionHistory,
                    request.CurrentSessionNumber);
                exercises.Add(new ScheduledTracking(
                    _trackingCatalog.Get(exerciseId).Definition,
                    progression.Parameters,
                    _durationEstimator.Estimate(exerciseId, progression.Parameters)));
            }

            return exercises;
        }

        private List<string> BuildAvailableTrackingIds(
            IReadOnlyList<string> unlockedBefore,
            IReadOnlyList<string> newTrackingIds)
        {
            var available = new List<string>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            AddTracking(unlockedBefore, available, unique);
            AddTracking(newTrackingIds, available, unique);
            return available;
        }

        private void AddTracking(
            IReadOnlyList<string> source,
            ICollection<string> destination,
            ISet<string> unique)
        {
            for (var index = 0; index < source.Count; index++)
            {
                string id = source[index];
                if (_trackingCatalog.IsTracking(id) && unique.Add(id))
                {
                    destination.Add(id);
                }
            }
        }

        private List<string> SelectTracking(IReadOnlyList<string> exerciseIds)
        {
            var selected = new List<string>();
            for (var index = 0; index < exerciseIds.Count; index++)
            {
                if (_trackingCatalog.IsTracking(exerciseIds[index]))
                {
                    selected.Add(exerciseIds[index]);
                }
            }

            return selected;
        }

        private static List<PlannedExercise> BuildOrderedPlan(
            IReadOnlyList<ScheduledTracking> returning,
            IReadOnlyList<ScheduledTracking> newlyUnlocked,
            ScheduledAdditional additional,
            bool includeLandolt)
        {
            var exercises = new List<PlannedExercise>();
            var order = 0;
            exercises.Add(new PlannedExercise(
                SessionSchedulingDefinitions.PreparationBasic,
                SessionSchedulingDefinitions.PreparationBasic.EstimatedDuration.Value,
                order++,
                SessionExerciseRole.Preparation));

            int interleavedCount = Math.Max(returning.Count, newlyUnlocked.Count);
            for (var index = 0; index < interleavedCount; index++)
            {
                if (index < returning.Count)
                {
                    exercises.Add(CreatePlannedTracking(returning[index], order++));
                }

                if (index < newlyUnlocked.Count)
                {
                    exercises.Add(CreatePlannedTracking(newlyUnlocked[index], order++));
                }
            }

            if (additional != null)
            {
                exercises.Add(new PlannedExercise(
                    additional.Definition,
                    additional.Duration,
                    order++,
                    SessionExerciseRole.Main,
                    additional.Parameters));
            }

            if (includeLandolt)
            {
                exercises.Add(new PlannedExercise(
                    SessionSchedulingDefinitions.LandoltStandard,
                    SessionSchedulingDefinitions.LandoltStandard.EstimatedDuration.Value,
                    order,
                    SessionExerciseRole.Final));
            }

            return exercises;
        }

        private static bool Contains(IReadOnlyList<string> ids, string expectedId)
        {
            for (var index = 0; index < ids.Count; index++)
            {
                if (string.Equals(ids[index], expectedId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private string SelectNewAdditionalExercise(IReadOnlyList<string> ids)
        {
            string selected = null;
            foreach (string id in ids)
            {
                if (!_majorUnlockPacePolicy.IsRelevantExercise(id)) continue;
                if (selected != null) throw new InvalidOperationException("A session cannot unlock two non-Tracking exercises.");
                selected = id;
            }
            return selected;
        }

        private int CountKnownAdditionalExercises(IReadOnlyList<string> ids)
        {
            var count = 0;
            foreach (string id in ids)
                if (_majorUnlockPacePolicy.IsRelevantExercise(id)) count++;
            return count;
        }

        private int? FindNextAdditionalUnlockSession(int currentSessionNumber)
        {
            foreach (UnlockStage stage in _unlockService.Plan.Stages)
            {
                if (stage.RequiredCompletedSessions <= currentSessionNumber) continue;
                foreach (string id in stage.ExerciseIds)
                    if (_majorUnlockPacePolicy.IsRelevantExercise(id))
                        return stage.RequiredCompletedSessions;
            }
            return null;
        }

        private ScheduledAdditional BuildAdditional(string exerciseId, bool isNew, SessionScheduleRequest request)
        {
            if (string.Equals(exerciseId, ExerciseIds.SaccadesNumberJourney, StringComparison.Ordinal))
            {
                NumberJourneyLevelSettings settings = _numberJourneyProgressionService.GetState(
                    request.NumberJourneyProgressionHistory, request.CurrentSessionNumber).Settings;
                return new ScheduledAdditional(isNew ? SessionSchedulingDefinitions.SaccadesNumberJourney
                    : SessionSchedulingDefinitions.SaccadesNumberJourneyReturning, settings,
                    settings.EstimatedDuration, isNew);
            }
            if (string.Equals(exerciseId, ExerciseIds.VisualSearchShapeSearch, StringComparison.Ordinal))
            {
                ShapeSearchLevelSettings settings = _shapeSearchProgressionService.GetState(
                    request.ShapeSearchProgressionHistory, request.CurrentSessionNumber).Settings;
                return new ScheduledAdditional(isNew ? SessionSchedulingDefinitions.VisualSearchShapeSearch
                    : SessionSchedulingDefinitions.VisualSearchShapeSearchReturning, settings,
                    settings.EstimatedDuration, isNew);
            }
            if (string.Equals(exerciseId, ExerciseIds.PeripheralEdgeSignals, StringComparison.Ordinal))
            {
                EdgeSignalsLevelSettings settings = _edgeSignalsProgressionService.GetState(
                    request.EdgeSignalsProgressionHistory, request.CurrentSessionNumber).Settings;
                return new ScheduledAdditional(isNew ? SessionSchedulingDefinitions.PeripheralEdgeSignals
                    : SessionSchedulingDefinitions.PeripheralEdgeSignalsReturning, settings,
                    settings.EstimatedDuration, isNew);
            }
            throw new ArgumentException("Unsupported non-Tracking exercise.", nameof(exerciseId));
        }

        private static PlannedExercise CreatePlannedTracking(
            ScheduledTracking tracking,
            int order)
        {
            return new PlannedExercise(
                tracking.Definition,
                tracking.Duration,
                order,
                SessionExerciseRole.Main,
                tracking.Parameters);
        }

        private static TimeSpan SumDuration(IReadOnlyList<ScheduledTracking> exercises)
        {
            var duration = TimeSpan.Zero;
            for (var index = 0; index < exercises.Count; index++)
            {
                duration += exercises[index].Duration;
            }

            return duration;
        }

        private static SessionScheduleResult CreateFailureResult(
            UnlockState state,
            TimeSpan hardLimit,
            int selectedReturningCount)
        {
            return new SessionScheduleResult(
                SessionScheduleStatus.CannotFitRequiredContent,
                null,
                state.NewlyUnlockedExerciseIds,
                state.NewlyUnlockedFamilies,
                hardLimit,
                TargetReturningTrackingCount,
                0,
                selectedReturningCount > 0);
        }

        private sealed class ScheduledTracking
        {
            public ScheduledTracking(
                ExerciseDefinition definition,
                TrackingExerciseParameters parameters,
                TimeSpan duration)
            {
                Definition = definition;
                Parameters = parameters;
                Duration = duration;
            }

            public ExerciseDefinition Definition { get; }

            public TrackingExerciseParameters Parameters { get; }

            public TimeSpan Duration { get; }
        }

        private sealed class ScheduledAdditional
        {
            public ScheduledAdditional(ExerciseDefinition definition, IExerciseParameters parameters,
                TimeSpan duration, bool isNew)
            { Definition = definition; Parameters = parameters; Duration = duration; IsNew = isNew; }
            public ExerciseDefinition Definition { get; }
            public IExerciseParameters Parameters { get; }
            public TimeSpan Duration { get; }
            public bool IsNew { get; }
        }
    }
}
