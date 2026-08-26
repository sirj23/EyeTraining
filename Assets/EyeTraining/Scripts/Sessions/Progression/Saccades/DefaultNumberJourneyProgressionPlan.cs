namespace EyeTraining.Sessions.Progression.Saccades
{
    public static class DefaultNumberJourneyProgressionPlan
    {
        public const int RequiredCompletedExecutionsPerLevel = 3;

        public static NumberJourneyProgressionPlan Create()
        {
            return new NumberJourneyProgressionPlan(
                new[]
                {
                    new NumberJourneyLevelSettings(0, 9, 5, 0.75f, 0.25f, 0.38f),
                    new NumberJourneyLevelSettings(1, 9, 6, 0.75f, 0.25f, 0.38f),
                    new NumberJourneyLevelSettings(2, 10, 6, 0.70f, 0.25f, 0.40f),
                    new NumberJourneyLevelSettings(3, 10, 7, 0.65f, 0.20f, 0.40f),
                    new NumberJourneyLevelSettings(4, 12, 7, 0.60f, 0.20f, 0.42f),
                    new NumberJourneyLevelSettings(5, 12, 8, 0.55f, 0.20f, 0.42f)
                },
                RequiredCompletedExecutionsPerLevel);
        }
    }
}
