using System;
using EyeTraining.Core;
using EyeTraining.Exercises;
using EyeTraining.Exercises.Landolt;
using EyeTraining.Profiles;
using EyeTraining.Save;
using EyeTraining.Sessions.History;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Rotation;
using EyeTraining.Sessions.Scheduling;
using EyeTraining.Sessions.Unlocking;
using EyeTraining.UI;
using UnityEngine;

namespace EyeTraining.Sessions.Runtime
{
    public sealed class SessionRuntimeController : MonoBehaviour
    {
        [SerializeField] private ProfileScreenController profileScreenController;
        [SerializeField] private PreparationController preparationController;
        [SerializeField] private TrackingExerciseController trackingExerciseController;
        [SerializeField] private LandoltExerciseController landoltExerciseController;

        [Header("Development")]
        [SerializeField] private SessionDebugMode debugMode;
        [Tooltip("Temporary fallback. Must be disabled before production.")]
        [SerializeField] private bool skipUnsupportedExercisesInDevelopment = true;

        private ITrainingHistoryRepository repository;
        private SessionScheduler scheduler;
        private TrackingExerciseCatalog trackingCatalog;
        private UserProfile activeProfile;
        private SessionGuidanceMode guidanceMode;
        private DateTimeOffset pendingSessionStartDate;
        private bool currentStepResultReceived;
        private bool advancing;
        private bool skippedUnsupportedExercise;
        private bool debugLandoltOnlyActive;

        public SessionRuntimePhase Phase { get; private set; } = SessionRuntimePhase.Inactive;

        public bool HasPreparedSession => Phase == SessionRuntimePhase.Prepared;

        public SessionPlan CurrentPlan { get; private set; }

        public int CurrentExerciseIndex { get; private set; } = -1;

        public PlannedExercise CurrentPlannedExercise { get; private set; }

        public int CurrentSessionNumber { get; private set; }

        public TrainingHistorySnapshot PendingSnapshot { get; private set; }

        public SessionScheduleResult PreparedScheduleResult { get; private set; }

        public string PreparedProfileId => activeProfile?.Id;

        public event Action PreparedSessionChanged;

        private void Awake()
        {
            profileScreenController ??= GetComponent<ProfileScreenController>();
            preparationController ??= GetComponent<PreparationController>();
            trackingExerciseController ??= GetComponent<TrackingExerciseController>();
            landoltExerciseController ??= GetComponent<LandoltExerciseController>();

            repository = new JsonTrainingHistoryRepository();
            trackingCatalog = new TrackingExerciseCatalog();
            var progressionService = new TrackingProgressionService(
                DefaultTrackingProgressionPlan.Create());
            scheduler = new SessionScheduler(
                new UnlockService(DefaultUnlockPlan.Create()),
                new RotationService(new TrackingRotationCatalog()),
                progressionService,
                trackingCatalog,
                new ReferenceTrackingDurationEstimator(trackingCatalog),
                new DefaultLandoltSchedulePolicy());

            preparationController.Completed += HandlePreparationCompleted;
            preparationController.ReturnedToModeSelection += HandleReturnedToModeSelection;
            trackingExerciseController.ResultReady += HandleTrackingResult;
            trackingExerciseController.ContinueRequested += HandleContinueRequested;
            landoltExerciseController.ResultReady += HandleLandoltResult;
            landoltExerciseController.ContinueRequested += HandleContinueRequested;
        }

        private void OnDestroy()
        {
            if (preparationController != null)
            {
                preparationController.Completed -= HandlePreparationCompleted;
                preparationController.ReturnedToModeSelection -= HandleReturnedToModeSelection;
            }

            if (trackingExerciseController != null)
            {
                trackingExerciseController.ResultReady -= HandleTrackingResult;
                trackingExerciseController.ContinueRequested -= HandleContinueRequested;
            }

            if (landoltExerciseController != null)
            {
                landoltExerciseController.ResultReady -= HandleLandoltResult;
                landoltExerciseController.ContinueRequested -= HandleContinueRequested;
            }
        }

        public bool PrepareSession()
        {
            UserProfile profile = profileScreenController.ActiveProfile;
            if (profile == null)
            {
                return Fail("Cannot prepare a session without an active profile.");
            }

            if (HasPreparedSession
                && CurrentPlan != null
                && string.Equals(activeProfile?.Id, profile.Id, StringComparison.Ordinal))
            {
                activeProfile = profile;
                return true;
            }

            if (!repository.TryLoad(profile.Id, out TrainingHistorySnapshot snapshot))
            {
                return Fail($"Could not load training history for profile '{profile.Id}'.");
            }

            try
            {
                int currentSessionNumber = snapshot.State.CompletedSessionCount + 1;
                var request = new SessionScheduleRequest(
                    currentSessionNumber,
                    snapshot.State.CompletedSessionCount,
                    TrainingHistoryMapper.ToRotationHistory(snapshot),
                    TrainingHistoryMapper.ToTrackingProgressionHistory(snapshot));
                SessionScheduleResult result = scheduler.Schedule(request);
                if (!result.IsSuccess)
                {
                    return Fail(
                        $"Session {currentSessionNumber} cannot fit its required content.");
                }

                activeProfile = profile;
                PreparedScheduleResult = result;
                CurrentSessionNumber = currentSessionNumber;
                CurrentPlan = result.Plan;
                PendingSnapshot = snapshot;
                CurrentExerciseIndex = -1;
                CurrentPlannedExercise = null;
                currentStepResultReceived = false;
                skippedUnsupportedExercise = false;
                Phase = SessionRuntimePhase.Prepared;
                Debug.Log(
                    $"[SessionRuntime] Prepared session {CurrentSessionNumber}, exercises: "
                    + CurrentPlan.Exercises.Count);
                return true;
            }
            catch (Exception exception)
            {
                return Fail("Could not prepare session: " + exception.Message);
            }
        }

        public void UpdatePreparedProfile(UserProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (activeProfile == null)
            {
                return;
            }

            if (!string.Equals(activeProfile.Id, profile.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Cannot update a prepared session with a different profile.");
            }

            activeProfile = profile;
        }

        public void StartPreparedSession(SessionGuidanceMode selectedGuidanceMode)
        {
            if (Phase != SessionRuntimePhase.Prepared)
            {
                Fail("No prepared session is available to start.");
                return;
            }

            guidanceMode = selectedGuidanceMode;
            pendingSessionStartDate = DateTimeOffset.Now;
            if (debugMode == SessionDebugMode.LandoltOnly)
            {
                StartDebugLandoltOnly();
                return;
            }

            AdvanceToNextExercise();
        }

        private void StartDebugLandoltOnly()
        {
            debugLandoltOnlyActive = true;
            currentStepResultReceived = false;
            CurrentExerciseIndex = -1;
            CurrentPlannedExercise = null;
            Phase = SessionRuntimePhase.RunningExercise;
            Debug.Log("[SessionRuntime] Development mode: starting Landolt only; persistence disabled.");
            landoltExerciseController.Begin(
                guidanceMode,
                CurrentSessionNumber,
                GetActiveLandoltBackgroundMode());
        }

        public void AbortSession()
        {
            if (Phase == SessionRuntimePhase.Inactive || Phase == SessionRuntimePhase.Completed)
            {
                return;
            }

            Phase = SessionRuntimePhase.Aborted;
            ClearPendingSession();
            Debug.LogWarning("[SessionRuntime] Session aborted. Pending history was discarded.");
            PreparedSessionChanged?.Invoke();
        }

        public void ClearPreparedSession()
        {
            if (Phase == SessionRuntimePhase.Preparing
                || Phase == SessionRuntimePhase.RunningExercise
                || Phase == SessionRuntimePhase.WaitingForContinue
                || Phase == SessionRuntimePhase.Completing)
            {
                throw new InvalidOperationException(
                    "An active session must be aborted instead of clearing its prepared plan.");
            }

            ClearPendingSession();
            Phase = SessionRuntimePhase.Inactive;
        }

        private void AdvanceToNextExercise()
        {
            if (advancing || CurrentPlan == null)
            {
                return;
            }

            advancing = true;
            try
            {
                while (++CurrentExerciseIndex < CurrentPlan.Exercises.Count)
                {
                    CurrentPlannedExercise = CurrentPlan.Exercises[CurrentExerciseIndex];
                    currentStepResultReceived = false;

                    if (CurrentPlannedExercise.Definition.Family == ExerciseFamily.Preparation)
                    {
                        Phase = SessionRuntimePhase.Preparing;
                        preparationController.Begin(guidanceMode);
                        return;
                    }

                    if (CurrentPlannedExercise.Definition.Family == ExerciseFamily.Tracking)
                    {
                        StartTracking(CurrentPlannedExercise);
                        return;
                    }

                    if (CurrentPlannedExercise.Definition.Family == ExerciseFamily.LandoltC
                        && string.Equals(
                            CurrentPlannedExercise.Definition.Id,
                            SessionSchedulingDefinitions.LandoltStandardId,
                            StringComparison.Ordinal))
                    {
                        StartLandolt();
                        return;
                    }

                    if (!skipUnsupportedExercisesInDevelopment)
                    {
                        Fail(
                            "Unsupported exercise: "
                            + CurrentPlannedExercise.Definition.Id);
                        trackingExerciseController.ShowSessionError(
                            "To ćwiczenie nie jest jeszcze obsługiwane.");
                        return;
                    }

                    skippedUnsupportedExercise = true;
                    Debug.LogWarning(
                        "[SessionRuntime] Skipping unsupported exercise: "
                        + CurrentPlannedExercise.Definition.Id);
                }

                CompleteSession();
            }
            finally
            {
                advancing = false;
            }
        }

        private void StartTracking(PlannedExercise exercise)
        {
            if (exercise.Parameters is not TrackingExerciseParameters parameters)
            {
                Fail($"Tracking exercise '{exercise.Definition.Id}' has invalid parameters.");
                trackingExerciseController.ShowSessionError(
                    "Nie udało się uruchomić ćwiczenia.");
                return;
            }

            TrackingExerciseCatalogEntry catalogEntry = trackingCatalog.Get(
                exercise.Definition.Id);
            Phase = SessionRuntimePhase.RunningExercise;
            Debug.Log("[SessionRuntime] Starting: " + exercise.Definition.Id);
            trackingExerciseController.Begin(
                guidanceMode,
                catalogEntry.CreatePath(),
                exercise.Definition.DisplayName,
                parameters.PathVisibility,
                parameters.CycleCount,
                parameters.SpeedMultiplier);
        }

        private void StartLandolt()
        {
            Phase = SessionRuntimePhase.RunningExercise;
            Debug.Log("[SessionRuntime] Starting: " + CurrentPlannedExercise.Definition.Id);
            landoltExerciseController.Begin(
                guidanceMode,
                CurrentSessionNumber,
                GetActiveLandoltBackgroundMode());
        }

        private LandoltBackgroundMode GetActiveLandoltBackgroundMode()
        {
            if (activeProfile == null)
            {
                throw new InvalidOperationException(
                    "A session cannot start Landolt without an active profile.");
            }

            return activeProfile.LandoltBackgroundMode;
        }

        private void HandlePreparationCompleted()
        {
            if (Phase == SessionRuntimePhase.Preparing)
            {
                AdvanceToNextExercise();
            }
        }

        private void HandleReturnedToModeSelection()
        {
            if (Phase != SessionRuntimePhase.Preparing)
            {
                return;
            }

            CurrentExerciseIndex = -1;
            CurrentPlannedExercise = null;
            Phase = SessionRuntimePhase.Prepared;
        }

        private void HandleTrackingResult(TrackingExerciseResult result)
        {
            if (Phase != SessionRuntimePhase.RunningExercise || currentStepResultReceived)
            {
                return;
            }

            currentStepResultReceived = true;
            Debug.Log(
                $"[SessionRuntime] Result: {CurrentPlannedExercise.Definition.Id} / "
                + $"{result.CompletionStatus} / {result.Feedback}");

            if (result.CompletionStatus == ExerciseCompletionStatus.Interrupted)
            {
                // Current UX treats "Przerwij" as aborting the whole session.
                AbortSession();
                return;
            }

            var parameters = (TrackingExerciseParameters)CurrentPlannedExercise.Parameters;
            var entry = new ExerciseHistoryEntry(
                activeProfile.Id,
                CurrentPlannedExercise.Definition.Id,
                CurrentSessionNumber,
                parameters.Level,
                result.CompletionStatus,
                result.Feedback,
                DateTimeOffset.Now);
            PendingSnapshot = PendingSnapshot.WithEntry(entry);
            Phase = SessionRuntimePhase.WaitingForContinue;
        }

        private void HandleLandoltResult(LandoltExerciseResult result)
        {
            if (Phase != SessionRuntimePhase.RunningExercise || currentStepResultReceived)
            {
                return;
            }

            if (debugLandoltOnlyActive)
            {
                currentStepResultReceived = true;
                Debug.Log(
                    $"[SessionRuntime] Development Landolt result: {result.CompletionStatus} / "
                    + $"{result.CorrectAnswers}/{result.ExposureCount}; not persisted.");
                if (result.CompletionStatus == ExerciseCompletionStatus.Interrupted)
                {
                    AbortSession();
                }
                else
                {
                    Phase = SessionRuntimePhase.WaitingForContinue;
                }

                return;
            }

            if (CurrentPlannedExercise == null
                || CurrentPlannedExercise.Definition.Family != ExerciseFamily.LandoltC)
            {
                return;
            }

            currentStepResultReceived = true;
            Debug.Log(
                $"[SessionRuntime] Result: {CurrentPlannedExercise.Definition.Id} / "
                + $"{result.CompletionStatus} / {result.CorrectAnswers}/{result.ExposureCount}");

            if (result.CompletionStatus == ExerciseCompletionStatus.Interrupted)
            {
                AbortSession();
                return;
            }

            var details = new LandoltExerciseHistoryDetails(
                result.CorrectAnswers,
                result.ErrorCount,
                result.ExposureCount,
                result.HighestLevel,
                result.FinalLevel,
                result.BackgroundMode,
                result.DirectionMode);
            var entry = new ExerciseHistoryEntry(
                activeProfile.Id,
                CurrentPlannedExercise.Definition.Id,
                CurrentSessionNumber,
                null,
                result.CompletionStatus,
                ExerciseFeedback.None,
                DateTimeOffset.Now,
                details);
            PendingSnapshot = PendingSnapshot.WithEntry(entry);
            Phase = SessionRuntimePhase.WaitingForContinue;
        }

        private void HandleContinueRequested()
        {
            if (Phase != SessionRuntimePhase.WaitingForContinue || !currentStepResultReceived)
            {
                return;
            }

            if (debugLandoltOnlyActive)
            {
                landoltExerciseController.ExitToHome();
                Phase = SessionRuntimePhase.Aborted;
                ClearPendingSession();
                Debug.Log("[SessionRuntime] Development Landolt-only run completed without saving.");
                PreparedSessionChanged?.Invoke();
                return;
            }

            AdvanceToNextExercise();
        }

        private void CompleteSession()
        {
            Phase = SessionRuntimePhase.Completing;
            DateTimeOffset completedAt = DateTimeOffset.Now;
            DateTimeOffset startDate = PendingSnapshot.State.TrainingStartDate
                ?? pendingSessionStartDate;
            var completedState = new TrainingProfileState(
                activeProfile.Id,
                startDate,
                CurrentSessionNumber,
                completedAt);
            PendingSnapshot = PendingSnapshot.WithState(completedState);

            if (!repository.Save(PendingSnapshot))
            {
                Phase = SessionRuntimePhase.Error;
                Debug.LogError(
                    $"[SessionRuntime] Session {CurrentSessionNumber} could not be saved. "
                    + "Pending data remains in memory.");
                trackingExerciseController.ShowSessionError(
                    "Nie udało się zapisać treningu.");
                return;
            }

            Phase = SessionRuntimePhase.Completed;
            if (skippedUnsupportedExercise)
            {
                Debug.LogWarning(
                    "[SessionRuntime] Session completed with unsupported exercises "
                    + "skipped in development mode.");
            }

            Debug.Log(
                $"[SessionRuntime] Session {CurrentSessionNumber} saved successfully");
            trackingExerciseController.ShowSessionCompleted();
            PreparedSessionChanged?.Invoke();
        }

        private bool Fail(string message)
        {
            Phase = SessionRuntimePhase.Error;
            Debug.LogError("[SessionRuntime] " + message);
            return false;
        }

        private void ClearPendingSession()
        {
            CurrentPlan = null;
            CurrentPlannedExercise = null;
            CurrentExerciseIndex = -1;
            CurrentSessionNumber = 0;
            PendingSnapshot = null;
            PreparedScheduleResult = null;
            activeProfile = null;
            currentStepResultReceived = false;
            skippedUnsupportedExercise = false;
            debugLandoltOnlyActive = false;
        }
    }
}
