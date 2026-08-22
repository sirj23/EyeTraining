using EyeTraining.Exercises;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public static class DefaultTrackingProgressionPlan
    {
        public const int RequiredCompletedExecutionsPerLevel = 3;

        public static TrackingProgressionPlan Create()
        {
            return new TrackingProgressionPlan(
                new[]
                {
                    new TrackingProgressionLevelDefinition(0, TrackingPathVisibility.Clear, 1f, 1f),
                    new TrackingProgressionLevelDefinition(1, TrackingPathVisibility.Subtle, 1f, 1f),
                    new TrackingProgressionLevelDefinition(2, TrackingPathVisibility.VerySubtle, 1f, 1f),
                    new TrackingProgressionLevelDefinition(3, TrackingPathVisibility.Hidden, 1f, 1f),
                    new TrackingProgressionLevelDefinition(4, TrackingPathVisibility.Hidden, 1.5f, 1f),
                    new TrackingProgressionLevelDefinition(5, TrackingPathVisibility.Hidden, 2f, 1f)
                },
                RequiredCompletedExecutionsPerLevel);
        }
    }
}
