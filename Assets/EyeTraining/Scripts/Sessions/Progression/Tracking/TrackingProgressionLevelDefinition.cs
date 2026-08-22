using System;
using EyeTraining.Exercises;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionLevelDefinition
    {
        public TrackingProgressionLevelDefinition(
            int level,
            TrackingPathVisibility pathVisibility,
            float cycleCount,
            float speedMultiplier)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (!Enum.IsDefined(typeof(TrackingPathVisibility), pathVisibility))
            {
                throw new ArgumentOutOfRangeException(nameof(pathVisibility));
            }

            if (cycleCount <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleCount));
            }

            if (speedMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
            }

            Level = level;
            PathVisibility = pathVisibility;
            CycleCount = cycleCount;
            SpeedMultiplier = speedMultiplier;
        }

        public int Level { get; }

        public TrackingPathVisibility PathVisibility { get; }

        public float CycleCount { get; }

        public float SpeedMultiplier { get; }

        public TrackingExerciseParameters CreateParameters()
        {
            return new TrackingExerciseParameters(
                PathVisibility,
                CycleCount,
                SpeedMultiplier,
                Level);
        }
    }
}
