using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltExerciseResult
    {
        public LandoltExerciseResult(
            ExerciseCompletionStatus completionStatus,
            int correctAnswers,
            int errorCount,
            int exposureCount,
            int highestLevel,
            int finalLevel,
            LandoltBackgroundMode backgroundMode,
            LandoltDirectionMode directionMode)
        {
            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus)
                || !Enum.IsDefined(typeof(LandoltBackgroundMode), backgroundMode)
                || !Enum.IsDefined(typeof(LandoltDirectionMode), directionMode))
            {
                throw new ArgumentOutOfRangeException();
            }

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

            CompletionStatus = completionStatus;
            CorrectAnswers = correctAnswers;
            ErrorCount = errorCount;
            ExposureCount = exposureCount;
            HighestLevel = highestLevel;
            FinalLevel = finalLevel;
            BackgroundMode = backgroundMode;
            DirectionMode = directionMode;
        }

        public ExerciseCompletionStatus CompletionStatus { get; }
        public int CorrectAnswers { get; }
        public int ErrorCount { get; }
        public int ExposureCount { get; }
        public int HighestLevel { get; }
        public int FinalLevel { get; }
        public LandoltBackgroundMode BackgroundMode { get; }
        public LandoltDirectionMode DirectionMode { get; }
    }
}
