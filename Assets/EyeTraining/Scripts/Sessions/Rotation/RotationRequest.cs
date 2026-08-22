using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class RotationRequest
    {
        public RotationRequest(
            int currentSessionNumber,
            IEnumerable<string> availableExerciseIds,
            IEnumerable<string> newlyUnlockedExerciseIds,
            RotationHistory history,
            int requestedCount)
        {
            if (currentSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentSessionNumber),
                    "Current session number must be positive.");
            }

            if (requestedCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedCount),
                    "Requested count cannot be negative.");
            }

            History = history ?? throw new ArgumentNullException(nameof(history));
            AvailableExerciseIds = CopyUniqueIds(availableExerciseIds, nameof(availableExerciseIds));
            NewlyUnlockedExerciseIds = CopyUniqueIds(
                newlyUnlockedExerciseIds,
                nameof(newlyUnlockedExerciseIds));

            ValidateReferences(
                currentSessionNumber,
                AvailableExerciseIds,
                NewlyUnlockedExerciseIds,
                History);

            CurrentSessionNumber = currentSessionNumber;
            RequestedCount = requestedCount;
        }

        public int CurrentSessionNumber { get; }

        public IReadOnlyList<string> AvailableExerciseIds { get; }

        public IReadOnlyList<string> NewlyUnlockedExerciseIds { get; }

        public RotationHistory History { get; }

        public int RequestedCount { get; }

        private static IReadOnlyList<string> CopyUniqueIds(IEnumerable<string> ids, string parameterName)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var copy = new List<string>();
            var uniqueIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (string id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Exercise ids cannot be null or empty.", parameterName);
                }

                if (!uniqueIds.Add(id))
                {
                    throw new ArgumentException("Exercise ids must be unique.", parameterName);
                }

                copy.Add(id);
            }

            return copy.AsReadOnly();
        }

        private static void ValidateReferences(
            int currentSessionNumber,
            IReadOnlyList<string> availableExerciseIds,
            IReadOnlyList<string> newlyUnlockedExerciseIds,
            RotationHistory history)
        {
            var availableIds = new HashSet<string>(availableExerciseIds, StringComparer.Ordinal);

            for (var index = 0; index < newlyUnlockedExerciseIds.Count; index++)
            {
                if (!availableIds.Contains(newlyUnlockedExerciseIds[index]))
                {
                    throw new ArgumentException(
                        "Newly unlocked exercises must belong to the available pool.",
                        nameof(newlyUnlockedExerciseIds));
                }
            }

            for (var index = 0; index < history.Usages.Count; index++)
            {
                ExerciseUsage usage = history.Usages[index];
                if (!availableIds.Contains(usage.ExerciseId))
                {
                    throw new ArgumentException(
                        "Rotation history contains an exercise outside the available pool.",
                        nameof(history));
                }

                if (usage.CompletedSessionNumber >= currentSessionNumber)
                {
                    throw new ArgumentException(
                        "Rotation history must contain only earlier completed sessions.",
                        nameof(history));
                }
            }
        }
    }
}
