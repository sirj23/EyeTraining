using System;

namespace EyeTraining.Exercises.Landolt
{
    public static class LandoltLevelPlan
    {
        private static readonly float[] DiameterMultipliers =
        {
            1f, 0.88f, 0.77f, 0.68f, 0.60f, 0.53f, 0.47f, 0.42f
        };

        public const int MinimumLevel = 0;
        public const int MaximumLevel = 7;

        public static float GetDiameterMultiplier(int level)
        {
            if (level < MinimumLevel || level > MaximumLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            return DiameterMultipliers[level];
        }
    }
}
