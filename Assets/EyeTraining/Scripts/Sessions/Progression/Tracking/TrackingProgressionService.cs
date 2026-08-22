using System;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionService
    {
        private readonly TrackingProgressionPlan _plan;

        public TrackingProgressionService(TrackingProgressionPlan plan)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public TrackingProgressionState GetState(
            string exerciseId,
            TrackingProgressionHistory history,
            int currentSessionNumber)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            if (currentSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            }

            ValidateHistory(history, currentSessionNumber);

            var currentLevelIndex = 0;
            var completedTowardsNextLevel = 0;

            for (var index = 0; index < history.Entries.Count; index++)
            {
                TrackingProgressionEntry entry = history.Entries[index];
                if (!string.Equals(entry.ExerciseId, exerciseId, StringComparison.Ordinal))
                {
                    continue;
                }

                TrackingProgressionLevelDefinition currentDefinition = _plan.Levels[currentLevelIndex];
                if (entry.AppliedLevel != currentDefinition.Level)
                {
                    throw new InvalidOperationException(
                        $"Progression history for '{exerciseId}' applies level {entry.AppliedLevel}, "
                        + $"but level {currentDefinition.Level} was expected at session "
                        + $"{entry.CompletedSessionNumber}.");
                }

                if (entry.CompletionStatus == ExerciseCompletionStatus.Interrupted)
                {
                    continue;
                }

                if (entry.Feedback == ExerciseFeedback.Difficult)
                {
                    completedTowardsNextLevel = 0;
                    continue;
                }

                if (currentLevelIndex == _plan.Levels.Count - 1)
                {
                    continue;
                }

                completedTowardsNextLevel++;
                if (completedTowardsNextLevel >= _plan.RequiredCompletedExecutionsPerLevel)
                {
                    currentLevelIndex++;
                    completedTowardsNextLevel = 0;
                }
            }

            bool isMaxLevel = currentLevelIndex == _plan.Levels.Count - 1;
            TrackingProgressionLevelDefinition definition = _plan.Levels[currentLevelIndex];
            int requiredForNextLevel = isMaxLevel
                ? 0
                : _plan.RequiredCompletedExecutionsPerLevel;
            float progress01 = isMaxLevel
                ? 1f
                : (float)completedTowardsNextLevel / requiredForNextLevel;

            return new TrackingProgressionState(
                definition.CreateParameters(),
                isMaxLevel ? 0 : completedTowardsNextLevel,
                requiredForNextLevel,
                progress01,
                isMaxLevel);
        }

        private void ValidateHistory(
            TrackingProgressionHistory history,
            int currentSessionNumber)
        {
            for (var index = 0; index < history.Entries.Count; index++)
            {
                TrackingProgressionEntry entry = history.Entries[index];
                if (!_plan.ContainsLevel(entry.AppliedLevel))
                {
                    throw new ArgumentException(
                        $"Progression history contains unknown level {entry.AppliedLevel}.",
                        nameof(history));
                }

                if (entry.CompletedSessionNumber >= currentSessionNumber)
                {
                    throw new ArgumentException(
                        "Progression history must contain only earlier completed sessions.",
                        nameof(history));
                }
            }
        }
    }
}
