using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionHistory
    {
        public TrackingProgressionHistory(IEnumerable<TrackingProgressionEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var copy = new List<TrackingProgressionEntry>(entries);
            Validate(copy);
            copy.Sort(CompareEntries);
            Entries = copy.AsReadOnly();
        }

        public IReadOnlyList<TrackingProgressionEntry> Entries { get; }

        private static void Validate(IReadOnlyList<TrackingProgressionEntry> entries)
        {
            var sessionsByExerciseId = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);

            for (var index = 0; index < entries.Count; index++)
            {
                TrackingProgressionEntry entry = entries[index];
                if (entry == null)
                {
                    throw new ArgumentException("Progression history cannot contain null entries.", nameof(entries));
                }

                if (!sessionsByExerciseId.TryGetValue(entry.ExerciseId, out HashSet<int> sessions))
                {
                    sessions = new HashSet<int>();
                    sessionsByExerciseId.Add(entry.ExerciseId, sessions);
                }

                if (!sessions.Add(entry.CompletedSessionNumber))
                {
                    throw new ArgumentException(
                        "An exercise can have only one progression entry per completed session.",
                        nameof(entries));
                }
            }
        }

        private static int CompareEntries(
            TrackingProgressionEntry left,
            TrackingProgressionEntry right)
        {
            int sessionComparison = left.CompletedSessionNumber.CompareTo(right.CompletedSessionNumber);
            if (sessionComparison != 0)
            {
                return sessionComparison;
            }

            return StringComparer.Ordinal.Compare(left.ExerciseId, right.ExerciseId);
        }
    }
}
