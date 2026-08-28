using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation.Returning
{
    public sealed class ReturningExerciseHistory
    {
        public ReturningExerciseHistory(IEnumerable<ReturningExerciseUsage> usages)
        {
            if (usages == null) throw new ArgumentNullException(nameof(usages));
            var copy = new List<ReturningExerciseUsage>(usages);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ReturningExerciseUsage usage in copy)
            {
                if (usage == null || !ids.Add(usage.ExerciseId)) throw new ArgumentException("Returning history is invalid.", nameof(usages));
            }
            copy.Sort((a, b) => StringComparer.Ordinal.Compare(a.ExerciseId, b.ExerciseId));
            Usages = copy.AsReadOnly();
        }
        public IReadOnlyList<ReturningExerciseUsage> Usages { get; }
    }
}
