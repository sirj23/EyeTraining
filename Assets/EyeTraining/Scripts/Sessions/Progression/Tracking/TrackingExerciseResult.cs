using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingExerciseResult
    {
        public TrackingExerciseResult(
            ExerciseCompletionStatus completionStatus,
            ExerciseFeedback feedback)
        {
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

            CompletionStatus = completionStatus;
            Feedback = feedback;
        }

        public ExerciseCompletionStatus CompletionStatus { get; }

        public ExerciseFeedback Feedback { get; }
    }
}
