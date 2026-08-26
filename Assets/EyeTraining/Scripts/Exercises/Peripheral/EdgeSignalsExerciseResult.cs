using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class EdgeSignalsExerciseResult
    {
        public EdgeSignalsExerciseResult(
            string exerciseId,
            ExerciseCompletionStatus completionStatus,
            int trialCount,
            int detectedCount,
            int missedCount,
            double? meanReactionTimeSeconds)
        {
            if (string.IsNullOrWhiteSpace(exerciseId)) throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));
            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus)) throw new ArgumentOutOfRangeException(nameof(completionStatus));
            if (trialCount < 0 || detectedCount < 0 || missedCount < 0 || detectedCount + missedCount != trialCount) throw new ArgumentOutOfRangeException(nameof(trialCount));
            if (meanReactionTimeSeconds.HasValue && meanReactionTimeSeconds.Value < 0d) throw new ArgumentOutOfRangeException(nameof(meanReactionTimeSeconds));
            ExerciseId = exerciseId;
            CompletionStatus = completionStatus;
            TrialCount = trialCount;
            DetectedCount = detectedCount;
            MissedCount = missedCount;
            MeanReactionTimeSeconds = meanReactionTimeSeconds;
        }

        public string ExerciseId { get; }
        public ExerciseCompletionStatus CompletionStatus { get; }
        public int TrialCount { get; }
        public int DetectedCount { get; }
        public int MissedCount { get; }
        public double? MeanReactionTimeSeconds { get; }
    }
}
