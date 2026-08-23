using System;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltPreferences
    {
        public LandoltPreferences(LandoltBackgroundMode backgroundMode)
        {
            if (!Enum.IsDefined(typeof(LandoltBackgroundMode), backgroundMode))
            {
                throw new ArgumentOutOfRangeException(nameof(backgroundMode));
            }

            BackgroundMode = backgroundMode;
        }

        public LandoltBackgroundMode BackgroundMode { get; }

        public static LandoltPreferences Default { get; } =
            new(LandoltBackgroundMode.Dark);
    }
}
