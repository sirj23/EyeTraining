using System;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class ExerciseUsage
    {
        public ExerciseUsage(string exerciseId, int completedSessionNumber)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (completedSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(completedSessionNumber),
                    "Completed session number must be positive.");
            }

            ExerciseId = exerciseId;
            CompletedSessionNumber = completedSessionNumber;
        }

        public string ExerciseId { get; }

        public int CompletedSessionNumber { get; }
    }
}
