using System;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionEntry
    {
        public TrackingProgressionEntry(
            string exerciseId,
            int completedSessionNumber,
            int appliedLevel,
            ExerciseCompletionStatus completionStatus,
            ExerciseFeedback feedback)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (completedSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionNumber));
            }

            if (appliedLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(appliedLevel));
            }

            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(completionStatus));
            }

            if (!Enum.IsDefined(typeof(ExerciseFeedback), feedback))
            {
                throw new ArgumentOutOfRangeException(nameof(feedback));
            }

            ExerciseId = exerciseId;
            CompletedSessionNumber = completedSessionNumber;
            AppliedLevel = appliedLevel;
            CompletionStatus = completionStatus;
            Feedback = feedback;
        }

        public string ExerciseId { get; }

        public int CompletedSessionNumber { get; }

        public int AppliedLevel { get; }

        public ExerciseCompletionStatus CompletionStatus { get; }

        public ExerciseFeedback Feedback { get; }
    }
}
