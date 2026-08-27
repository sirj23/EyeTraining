using System;

namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsLevelSettings : IExerciseParameters
    {
        public const float CountdownStepDuration = 0.7f;
        public const float InitialDelayMean = 1.75f;
        public const float FollowingDelayMean = 1.70f;

        public EdgeSignalsLevelSettings(int level, float stimulusSizeViewportHeight,
            float stimulusVisibleDuration, float responseWindow,
            float horizontalOffsetViewport, float verticalOffsetViewport, int trialCount)
        {
            if (level < 0 || stimulusSizeViewportHeight <= 0f || stimulusVisibleDuration <= 0f
                || responseWindow < stimulusVisibleDuration || horizontalOffsetViewport <= 0f
                || verticalOffsetViewport <= 0f || trialCount < 8)
                throw new ArgumentOutOfRangeException(nameof(level));

            Level = level;
            StimulusSizeViewportHeight = stimulusSizeViewportHeight;
            StimulusVisibleDuration = stimulusVisibleDuration;
            ResponseWindow = responseWindow;
            HorizontalOffsetViewport = horizontalOffsetViewport;
            VerticalOffsetViewport = verticalOffsetViewport;
            TrialCount = trialCount;
            EstimatedDuration = TimeSpan.FromSeconds(
                (3f * CountdownStepDuration) + InitialDelayMean
                + ((trialCount - 1) * FollowingDelayMean) + (trialCount * responseWindow));
        }

        public int Level { get; }
        public float StimulusSizeViewportHeight { get; }
        public float StimulusVisibleDuration { get; }
        public float ResponseWindow { get; }
        public float HorizontalOffsetViewport { get; }
        public float VerticalOffsetViewport { get; }
        public int TrialCount { get; }
        public TimeSpan EstimatedDuration { get; }
    }
}
