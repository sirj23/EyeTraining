using System;
using System.Collections.Generic;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Progression.Saccades;
using EyeTraining.Sessions.Progression.VisualSearch;
using EyeTraining.Sessions.Progression.Peripheral;
using EyeTraining.Sessions.Rotation;
using EyeTraining.Sessions.Rotation.Returning;
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

        public static NumberJourneyProgressionHistory ToNumberJourneyProgressionHistory(
            TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var entries = new List<NumberJourneyProgressionEntry>();
            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                ExerciseHistoryEntry entry = snapshot.Entries[index];
                if (!string.Equals(
                        entry.ExerciseId,
                        ExerciseIds.SaccadesNumberJourney,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                // Entries created before Number Journey progression used the exact L0
                // parameters but did not persist AppliedLevel yet.
                int appliedLevel = entry.AppliedLevel ?? 0;
                entries.Add(new NumberJourneyProgressionEntry(
                    entry.CompletedSessionNumber,
                    appliedLevel,
                    entry.CompletionStatus));
            }

            return new NumberJourneyProgressionHistory(entries);
        }

        public static ShapeSearchProgressionHistory ToShapeSearchProgressionHistory(
            TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var entries = new List<ShapeSearchProgressionEntry>();
            foreach (ExerciseHistoryEntry entry in snapshot.Entries)
            {
                if (!string.Equals(entry.ExerciseId, ExerciseIds.VisualSearchShapeSearch, StringComparison.Ordinal))
                    continue;

                // Pre-progression Shape Search entries used the current L0 parameters.
                entries.Add(new ShapeSearchProgressionEntry(
                    entry.CompletedSessionNumber,
                    entry.AppliedLevel ?? 0,
                    entry.CompletionStatus));
            }
            return new ShapeSearchProgressionHistory(entries);
        }

        public static EdgeSignalsProgressionHistory ToEdgeSignalsProgressionHistory(TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var entries = new List<EdgeSignalsProgressionEntry>();
            foreach (ExerciseHistoryEntry entry in snapshot.Entries)
            {
                if (!string.Equals(entry.ExerciseId, ExerciseIds.PeripheralEdgeSignals, StringComparison.Ordinal)) continue;
                entries.Add(new EdgeSignalsProgressionEntry(entry.CompletedSessionNumber,
                    entry.AppliedLevel ?? 0, entry.CompletionStatus));
            }
            return new EdgeSignalsProgressionHistory(entries);
        }

        public static ReturningExerciseHistory ToReturningExerciseHistory(TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            var lastSessions = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (ExerciseHistoryEntry entry in snapshot.Entries)
            {
                if (entry.CompletionStatus != ExerciseCompletionStatus.Completed
                    || !IsReturningExercise(entry.ExerciseId)) continue;
                counts.TryGetValue(entry.ExerciseId, out int count);
                counts[entry.ExerciseId] = count + 1;
                if (!lastSessions.TryGetValue(entry.ExerciseId, out int last)
                    || entry.CompletedSessionNumber > last)
                    lastSessions[entry.ExerciseId] = entry.CompletedSessionNumber;
            }
            var usages = new List<ReturningExerciseUsage>();
            foreach (KeyValuePair<string, int> pair in counts)
                usages.Add(new ReturningExerciseUsage(pair.Key, pair.Value, lastSessions[pair.Key]));
            return new ReturningExerciseHistory(usages);
        }

        private static bool IsReturningExercise(string exerciseId) =>
            string.Equals(exerciseId, ExerciseIds.SaccadesNumberJourney, StringComparison.Ordinal)
            || string.Equals(exerciseId, ExerciseIds.VisualSearchShapeSearch, StringComparison.Ordinal)
            || string.Equals(exerciseId, ExerciseIds.PeripheralEdgeSignals, StringComparison.Ordinal);
    }
}
