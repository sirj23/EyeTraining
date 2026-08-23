using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.History
{
    public sealed class TrainingHistorySnapshot
    {
        public TrainingHistorySnapshot(
            TrainingProfileState state,
            IEnumerable<ExerciseHistoryEntry> entries)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var copy = new List<ExerciseHistoryEntry>(entries);
            ValidateEntries(state.ProfileId, copy);
            copy.Sort(CompareEntries);
            Entries = copy.AsReadOnly();
        }

        public TrainingProfileState State { get; }

        public IReadOnlyList<ExerciseHistoryEntry> Entries { get; }

        public static TrainingHistorySnapshot CreateNotStarted(string profileId)
        {
            return new TrainingHistorySnapshot(
                TrainingProfileState.CreateNotStarted(profileId),
                Array.Empty<ExerciseHistoryEntry>());
        }

        public TrainingHistorySnapshot WithState(TrainingProfileState state)
        {
            return new TrainingHistorySnapshot(state, Entries);
        }

        public TrainingHistorySnapshot WithEntry(ExerciseHistoryEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var updatedEntries = new List<ExerciseHistoryEntry>(Entries) { entry };
            return new TrainingHistorySnapshot(State, updatedEntries);
        }

        private static void ValidateEntries(
            string profileId,
            IReadOnlyList<ExerciseHistoryEntry> entries)
        {
            var exerciseSessions = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < entries.Count; index++)
            {
                ExerciseHistoryEntry entry = entries[index];
                if (entry == null)
                {
                    throw new ArgumentException("History cannot contain null entries.", nameof(entries));
                }

                if (!string.Equals(entry.ProfileId, profileId, StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Every history entry must belong to the snapshot profile.",
                        nameof(entries));
                }

                string uniqueKey = entry.CompletedSessionNumber + "\n" + entry.ExerciseId;
                if (!exerciseSessions.Add(uniqueKey))
                {
                    throw new ArgumentException(
                        $"Exercise '{entry.ExerciseId}' already exists in session "
                        + $"{entry.CompletedSessionNumber} for profile '{profileId}'.",
                        nameof(entries));
                }
            }
        }

        private static int CompareEntries(
            ExerciseHistoryEntry left,
            ExerciseHistoryEntry right)
        {
            int sessionComparison = left.CompletedSessionNumber.CompareTo(
                right.CompletedSessionNumber);
            if (sessionComparison != 0)
            {
                return sessionComparison;
            }

            int timeComparison = left.CompletedAt.CompareTo(right.CompletedAt);
            if (timeComparison != 0)
            {
                return timeComparison;
            }

            return StringComparer.Ordinal.Compare(left.ExerciseId, right.ExerciseId);
        }
    }
}
