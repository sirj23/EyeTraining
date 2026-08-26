using System;

namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public sealed class ShapeSearchLevelSettings : IExerciseParameters
    {
        public ShapeSearchLevelSettings(
            int level,
            int objectCount,
            int targetCount,
            float objectSizeViewportHeight,
            TimeSpan estimatedDuration)
        {
            if (level < 0 || objectCount <= 0 || targetCount <= 0
                || targetCount >= objectCount || objectSizeViewportHeight <= 0f
                || estimatedDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Level = level;
            ObjectCount = objectCount;
            TargetCount = targetCount;
            ObjectSizeViewportHeight = objectSizeViewportHeight;
            EstimatedDuration = estimatedDuration;
        }

        public int Level { get; }
        public int ObjectCount { get; }
        public int TargetCount { get; }
        public float ObjectSizeViewportHeight { get; }
        public TimeSpan EstimatedDuration { get; }
    }
}
