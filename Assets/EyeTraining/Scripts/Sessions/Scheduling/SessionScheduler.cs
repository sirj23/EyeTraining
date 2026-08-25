using System;
using System.Collections.Generic;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Rotation;
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
        private readonly TrackingExerciseCatalog _trackingCatalog;
        private readonly ITrackingDurationEstimator _durationEstimator;
        private readonly ILandoltSchedulePolicy _landoltSchedulePolicy;

        public SessionScheduler(
            UnlockService unlockService,
            RotationService rotationService,
            TrackingProgressionService progressionService,
            TrackingExerciseCatalog trackingCatalog,
            ITrackingDurationEstimator durationEstimator,
            ILandoltSchedulePolicy landoltSchedulePolicy)
        {
            _unlockService = unlockService ?? throw new ArgumentNullException(nameof(unlockService));
            _rotationService = rotationService ?? throw new ArgumentNullException(nameof(rotationService));
            _progressionService = progressionService
                ?? throw new ArgumentNullException(nameof(progressionService));
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
            bool includeNewNumberJourney = Contains(
                stateAtCurrentThreshold.NewlyUnlockedExerciseIds,
                SessionSchedulingDefinitions.SaccadesNumberJourneyId);
            bool includeLandolt = _landoltSchedulePolicy.ShouldSchedule(request.CurrentSessionNumber);

            TimeSpan requiredDuration = SessionSchedulingDefinitions.PreparationBasic.EstimatedDuration.Value
                + SumDuration(newlyUnlocked)
                + (includeNewNumberJourney
                    ? SessionSchedulingDefinitions.SaccadesNumberJourney.EstimatedDuration.Value
                    : TimeSpan.Zero)
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

            int selectedBeforeTimeReduction = returning.Count;
            while (requiredDuration + SumDuration(returning) > hardLimit)
            {
                returning.RemoveAt(returning.Count - 1);
            }

            List<PlannedExercise> exercises = BuildOrderedPlan(
                returning,
                newlyUnlocked,
                includeNewNumberJourney,
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
                returning.Count < selectedBeforeTimeReduction);
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
            bool includeNewNumberJourney,
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

            if (includeNewNumberJourney)
            {
                exercises.Add(new PlannedExercise(
                    SessionSchedulingDefinitions.SaccadesNumberJourney,
                    SessionSchedulingDefinitions.SaccadesNumberJourney.EstimatedDuration.Value,
                    order++,
                    SessionExerciseRole.Main));
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
    }
}
