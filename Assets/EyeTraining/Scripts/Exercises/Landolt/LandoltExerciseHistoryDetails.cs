using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltExerciseHistoryDetails : IExerciseHistoryDetails
    {
        public LandoltExerciseHistoryDetails(
            int correctAnswers,
            int errorCount,
            int exposureCount,
            int highestLevel,
            int finalLevel,
            LandoltBackgroundMode backgroundMode,
            LandoltDirectionMode directionMode)
        {
            if (correctAnswers < 0 || errorCount < 0 || exposureCount < 0
                || correctAnswers + errorCount != exposureCount)
            {
                throw new ArgumentOutOfRangeException(nameof(exposureCount));
            }

            if (highestLevel < 0 || highestLevel > LandoltLevelPlan.MaximumLevel
                || finalLevel < 0 || finalLevel > highestLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(highestLevel));
            }

            if (!Enum.IsDefined(typeof(LandoltBackgroundMode), backgroundMode)
                || !Enum.IsDefined(typeof(LandoltDirectionMode), directionMode))
            {
                throw new ArgumentOutOfRangeException();
            }

            CorrectAnswers = correctAnswers;
            ErrorCount = errorCount;
            ExposureCount = exposureCount;
            HighestLevel = highestLevel;
            FinalLevel = finalLevel;
            BackgroundMode = backgroundMode;
            DirectionMode = directionMode;
        }

        public int CorrectAnswers { get; }
        public int ErrorCount { get; }
        public int ExposureCount { get; }
        public int HighestLevel { get; }
        public int FinalLevel { get; }
        public LandoltBackgroundMode BackgroundMode { get; }
        public LandoltDirectionMode DirectionMode { get; }
    }
}
