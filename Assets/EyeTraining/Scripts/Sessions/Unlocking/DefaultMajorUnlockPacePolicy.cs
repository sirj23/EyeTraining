namespace EyeTraining.Sessions.Unlocking
{
    public static class DefaultMajorUnlockPacePolicy
    {
        public static MajorUnlockPacePolicy Create() => new(new[]
        {
            ExerciseIds.SaccadesNumberJourney,
            ExerciseIds.VisualSearchShapeSearch,
            ExerciseIds.PeripheralEdgeSignals
        });
    }
}
