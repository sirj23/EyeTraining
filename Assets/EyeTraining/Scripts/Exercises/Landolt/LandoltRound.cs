using System;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltRound
    {
        public const int RequiredCorrectAnswersPerLevel = 2;
        public const int MaximumErrors = 3;
        public const int MaximumExposures = 16;

        private readonly LandoltDirectionSequence directionSequence;

        public LandoltRound(int startLevel, int deterministicSeed)
        {
            if (startLevel < LandoltLevelPlan.MinimumLevel
                || startLevel > LandoltLevelPlan.MaximumLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(startLevel));
            }

            CurrentLevel = startLevel;
            HighestLevel = startLevel;
            directionSequence = new LandoltDirectionSequence(deterministicSeed);
            CurrentDirection = directionSequence.GetDirection(0);
        }

        public int CurrentLevel { get; private set; }
        public int HighestLevel { get; private set; }
        public int CorrectAnswers { get; private set; }
        public int ErrorCount { get; private set; }
        public int ExposureCount { get; private set; }
        public int CorrectAnswersAtCurrentLevel { get; private set; }
        public LandoltDirection CurrentDirection { get; private set; }
        public bool IsFinished => ErrorCount >= MaximumErrors || ExposureCount >= MaximumExposures;

        public bool SubmitAnswer(LandoltDirection answer)
        {
            if (!Enum.IsDefined(typeof(LandoltDirection), answer))
            {
                throw new ArgumentOutOfRangeException(nameof(answer));
            }

            if (IsFinished)
            {
                throw new InvalidOperationException("The Landolt round is already finished.");
            }

            bool isCorrect = answer == CurrentDirection;
            ExposureCount++;

            if (isCorrect)
            {
                CorrectAnswers++;
                CorrectAnswersAtCurrentLevel++;
                if (CorrectAnswersAtCurrentLevel >= RequiredCorrectAnswersPerLevel)
                {
                    CorrectAnswersAtCurrentLevel = 0;
                    if (CurrentLevel < LandoltLevelPlan.MaximumLevel)
                    {
                        CurrentLevel++;
                        HighestLevel = Math.Max(HighestLevel, CurrentLevel);
                    }
                }
            }
            else
            {
                ErrorCount++;
            }

            if (!IsFinished)
            {
                CurrentDirection = directionSequence.GetDirection(ExposureCount);
            }

            return isCorrect;
        }
    }
}
