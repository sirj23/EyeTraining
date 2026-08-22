using System;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class ExerciseRotationMetadata
    {
        public ExerciseRotationMetadata(string exerciseId, string groupId)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("Rotation group id cannot be empty.", nameof(groupId));
            }

            ExerciseId = exerciseId;
            GroupId = groupId;
        }

        public string ExerciseId { get; }

        public string GroupId { get; }
    }
}
