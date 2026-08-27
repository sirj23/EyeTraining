namespace EyeTraining.Sessions.Progression.Peripheral
{
    public static class DefaultEdgeSignalsProgressionPlan
    {
        public const int RequiredCompletedExecutionsPerLevel = 3;

        public static EdgeSignalsProgressionPlan Create() => new(
            new[]
            {
                new EdgeSignalsLevelSettings(0, .055f, .60f, .85f, .35f, .285f, 12),
                new EdgeSignalsLevelSettings(1, .052f, .58f, .82f, .35f, .285f, 12),
                new EdgeSignalsLevelSettings(2, .049f, .55f, .80f, .36f, .295f, 12),
                new EdgeSignalsLevelSettings(3, .046f, .52f, .77f, .36f, .300f, 14),
                new EdgeSignalsLevelSettings(4, .043f, .50f, .75f, .37f, .305f, 14),
                new EdgeSignalsLevelSettings(5, .040f, .48f, .72f, .37f, .310f, 16)
            }, RequiredCompletedExecutionsPerLevel);
    }
}
