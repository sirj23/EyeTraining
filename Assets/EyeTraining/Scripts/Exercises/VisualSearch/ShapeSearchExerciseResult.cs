using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchExerciseResult
    {
        public ShapeSearchExerciseResult(
            string exerciseId,
            ExerciseCompletionStatus completionStatus,
            int correctSelections,
            int errorCount,
            int targetCount)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));
            }

            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(completionStatus));
            }

            if (correctSelections < 0 || errorCount < 0 || targetCount <= 0
                || correctSelections > targetCount)
            {
                throw new ArgumentOutOfRangeException(nameof(correctSelections));
            }

            ExerciseId = exerciseId;
            CompletionStatus = completionStatus;
            CorrectSelections = correctSelections;
            ErrorCount = errorCount;
            TargetCount = targetCount;
        }

        public string ExerciseId { get; }

        public ExerciseCompletionStatus CompletionStatus { get; }

        public int CorrectSelections { get; }

        public int ErrorCount { get; }

        public int TargetCount { get; }
    }
}
