using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation.Returning
{
    public sealed class ReturningExerciseSelector
    {
        private readonly Dictionary<string, ReturningExercisePolicy> policies;
        public ReturningExerciseSelector(IEnumerable<ReturningExercisePolicy> policies)
        {
            if (policies == null) throw new ArgumentNullException(nameof(policies));
            this.policies = new Dictionary<string, ReturningExercisePolicy>(StringComparer.Ordinal);
            foreach (ReturningExercisePolicy policy in policies)
            {
                if (policy == null || !this.policies.TryAdd(policy.ExerciseId, policy))
                    throw new ArgumentException("Returning policies are invalid.", nameof(policies));
            }
        }

        public string Select(int currentSessionNumber, IReadOnlyList<string> availableExerciseIds,
            ReturningExerciseHistory history)
        {
            if (currentSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            if (availableExerciseIds == null) throw new ArgumentNullException(nameof(availableExerciseIds));
            if (history == null) throw new ArgumentNullException(nameof(history));
            var available = new HashSet<string>(availableExerciseIds, StringComparer.Ordinal);
            ReturningExerciseUsage best = null;
            foreach (ReturningExerciseUsage usage in history.Usages)
            {
                if (!available.Contains(usage.ExerciseId) || !policies.TryGetValue(usage.ExerciseId, out ReturningExercisePolicy policy)) continue;
                if (currentSessionNumber - usage.LastCompletedSessionNumber < policy.MinimumSessionGap) continue;
                if (best == null || Compare(usage, best) < 0) best = usage;
            }
            return best?.ExerciseId;
        }

        private static int Compare(ReturningExerciseUsage left, ReturningExerciseUsage right)
        {
            int comparison = left.LastCompletedSessionNumber.CompareTo(right.LastCompletedSessionNumber);
            if (comparison != 0) return comparison;
            comparison = left.UsageCount.CompareTo(right.UsageCount);
            return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left.ExerciseId, right.ExerciseId);
        }
    }
}
