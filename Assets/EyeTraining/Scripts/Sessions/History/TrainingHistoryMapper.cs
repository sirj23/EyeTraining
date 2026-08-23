using System;
using System.Collections.Generic;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Rotation;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.History
{
    public static class TrainingHistoryMapper
    {
        private static readonly HashSet<string> TrackingExerciseIds =
            new HashSet<string>(ExerciseIds.AllTracking, StringComparer.Ordinal);

        public static RotationHistory ToRotationHistory(TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var usages = new List<ExerciseUsage>(snapshot.Entries.Count);
            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                ExerciseHistoryEntry entry = snapshot.Entries[index];
                if (!TrackingExerciseIds.Contains(entry.ExerciseId))
                {
                    continue;
                }

                usages.Add(new ExerciseUsage(
                    entry.ExerciseId,
                    entry.CompletedSessionNumber));
            }

            return new RotationHistory(usages);
        }

        public static TrackingProgressionHistory ToTrackingProgressionHistory(
            TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var entries = new List<TrackingProgressionEntry>();
            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                ExerciseHistoryEntry entry = snapshot.Entries[index];
                if (!TrackingExerciseIds.Contains(entry.ExerciseId))
                {
                    continue;
                }

                if (!entry.AppliedLevel.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Tracking history entry '{entry.ExerciseId}' from session "
                        + $"{entry.CompletedSessionNumber} has no applied level.");
                }

                entries.Add(new TrackingProgressionEntry(
                    entry.ExerciseId,
                    entry.CompletedSessionNumber,
                    entry.AppliedLevel.Value,
                    entry.CompletionStatus,
                    entry.Feedback));
            }

            return new TrackingProgressionHistory(entries);
        }
    }
}
