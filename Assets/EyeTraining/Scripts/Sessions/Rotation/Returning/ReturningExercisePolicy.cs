using System;

namespace EyeTraining.Sessions.Rotation.Returning
{
    public sealed class ReturningExercisePolicy
    {
        public ReturningExercisePolicy(string exerciseId, int minimumSessionGap)
        {
            if (string.IsNullOrWhiteSpace(exerciseId)) throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));
            if (minimumSessionGap <= 0) throw new ArgumentOutOfRangeException(nameof(minimumSessionGap));
            ExerciseId = exerciseId;
            MinimumSessionGap = minimumSessionGap;
        }
        public string ExerciseId { get; }
        public int MinimumSessionGap { get; }
    }
}
