using System;

namespace EyeTraining.Sessions.Progression.Saccades
{
    public sealed class NumberJourneyLevelSettings : IExerciseParameters
    {
        public const float CountdownStepDuration = 0.7f;
        public const float CountdownDuration = 3f * CountdownStepDuration;
        public const float BetweenPhasesDuration = 1.25f;

        public NumberJourneyLevelSettings(
            int level,
            int numberCount,
            int sequenceLength,
            float activeDuration,
            float gapDuration,
            float preferredMinimumJump)
        {
            if (level < 0 || numberCount <= 0 || sequenceLength <= 0
                || sequenceLength > numberCount || activeDuration <= 0f
                || gapDuration < 0f || preferredMinimumJump <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Level = level;
            NumberCount = numberCount;
            SequenceLength = sequenceLength;
            ActiveDuration = activeDuration;
            GapDuration = gapDuration;
            PreferredMinimumJump = preferredMinimumJump;
        }

        public int Level { get; }

        public int NumberCount { get; }

        public int SequenceLength { get; }

        public float ActiveDuration { get; }

        public float GapDuration { get; }

        public float PreferredMinimumJump { get; }

        public TimeSpan EstimatedDuration
        {
            get
            {
                float oneSequence = (SequenceLength * ActiveDuration)
                    + ((SequenceLength - 1) * GapDuration);
                return TimeSpan.FromSeconds(
                    CountdownDuration + BetweenPhasesDuration + (2f * oneSequence));
            }
        }
    }
}
