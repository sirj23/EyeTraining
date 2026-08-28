using System;

namespace EyeTraining.Sessions.Rotation.Returning
{
    public sealed class ReturningExerciseUsage
    {
        public ReturningExerciseUsage(string exerciseId, int usageCount, int lastCompletedSessionNumber)
        {
            if (string.IsNullOrWhiteSpace(exerciseId)) throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));
            if (usageCount <= 0 || lastCompletedSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(usageCount));
            ExerciseId = exerciseId; UsageCount = usageCount; LastCompletedSessionNumber = lastCompletedSessionNumber;
        }
        public string ExerciseId { get; }
        public int UsageCount { get; }
        public int LastCompletedSessionNumber { get; }
    }
}
