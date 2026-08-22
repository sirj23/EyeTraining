using System;
using EyeTraining.Exercises;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingExerciseParameters : IExerciseParameters
    {
        public TrackingExerciseParameters(
            TrackingPathVisibility pathVisibility,
            float cycleCount,
            float speedMultiplier,
            int level)
        {
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

            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            PathVisibility = pathVisibility;
            CycleCount = cycleCount;
            SpeedMultiplier = speedMultiplier;
            Level = level;
        }

        public TrackingPathVisibility PathVisibility { get; }

        public float CycleCount { get; }

        public float SpeedMultiplier { get; }

        public int Level { get; }
    }
}
