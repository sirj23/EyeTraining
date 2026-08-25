using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Exercises.Saccades
{
    public sealed class SaccadesExerciseResult
    {
        public SaccadesExerciseResult(
            string exerciseId,
            ExerciseCompletionStatus completionStatus)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException(
                    "Exercise ID cannot be empty.",
                    nameof(exerciseId));
            }

            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(completionStatus));
            }

            ExerciseId = exerciseId;
            CompletionStatus = completionStatus;
        }

        public string ExerciseId { get; }

        public ExerciseCompletionStatus CompletionStatus { get; }
    }
}
