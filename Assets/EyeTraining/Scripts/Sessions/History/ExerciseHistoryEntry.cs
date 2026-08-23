using System;

namespace EyeTraining.Sessions.History
{
    public sealed class ExerciseHistoryEntry
    {
        public ExerciseHistoryEntry(
            string profileId,
            string exerciseId,
            int completedSessionNumber,
            int? appliedLevel,
            ExerciseCompletionStatus completionStatus,
            ExerciseFeedback feedback,
            DateTimeOffset completedAt,
            IExerciseHistoryDetails details = null)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException("Profile id cannot be empty.", nameof(profileId));
            }

            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (completedSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionNumber));
            }

            if (appliedLevel.HasValue && appliedLevel.Value < 0)
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

            if (completionStatus == ExerciseCompletionStatus.Interrupted
                && feedback != ExerciseFeedback.None)
            {
                throw new ArgumentException(
                    "An interrupted exercise cannot contain completion feedback.",
                    nameof(feedback));
            }

            if (completedAt == default)
            {
                throw new ArgumentOutOfRangeException(nameof(completedAt));
            }

            ProfileId = profileId;
            ExerciseId = exerciseId;
            CompletedSessionNumber = completedSessionNumber;
            AppliedLevel = appliedLevel;
            CompletionStatus = completionStatus;
            Feedback = feedback;
            CompletedAt = completedAt;
            Details = details;
        }

        public string ProfileId { get; }

        public string ExerciseId { get; }

        public int CompletedSessionNumber { get; }

        public int? AppliedLevel { get; }

        public ExerciseCompletionStatus CompletionStatus { get; }

        public ExerciseFeedback Feedback { get; }

        public DateTimeOffset CompletedAt { get; }

        public IExerciseHistoryDetails Details { get; }
    }
}
