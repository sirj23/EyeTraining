using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class RotationHistory
    {
        public RotationHistory(IEnumerable<ExerciseUsage> usages)
        {
            if (usages == null)
            {
                throw new ArgumentNullException(nameof(usages));
            }

            var copy = new List<ExerciseUsage>(usages);
            Validate(copy);
            Usages = copy.AsReadOnly();
        }

        public IReadOnlyList<ExerciseUsage> Usages { get; }

        private static void Validate(IReadOnlyList<ExerciseUsage> usages)
        {
            var sessionsByExerciseId = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);

            for (var index = 0; index < usages.Count; index++)
            {
                ExerciseUsage usage = usages[index];
                if (usage == null)
                {
                    throw new ArgumentException("Rotation history cannot contain null usages.", nameof(usages));
                }

                if (!sessionsByExerciseId.TryGetValue(
                    usage.ExerciseId,
                    out HashSet<int> completedSessions))
                {
                    completedSessions = new HashSet<int>();
                    sessionsByExerciseId.Add(usage.ExerciseId, completedSessions);
                }

                if (!completedSessions.Add(usage.CompletedSessionNumber))
                {
                    throw new ArgumentException(
                        "An exercise can occur only once in a completed session.",
                        nameof(usages));
                }
            }
        }
    }
}
