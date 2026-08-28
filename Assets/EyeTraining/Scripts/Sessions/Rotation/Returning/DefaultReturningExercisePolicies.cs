using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Rotation.Returning
{
    public static class DefaultReturningExercisePolicies
    {
        public static ReturningExerciseSelector CreateSelector() => new(new[]
        {
            new ReturningExercisePolicy(ExerciseIds.SaccadesNumberJourney, 2),
            new ReturningExercisePolicy(ExerciseIds.VisualSearchShapeSearch, 2),
            new ReturningExercisePolicy(ExerciseIds.PeripheralEdgeSignals, 3)
        });
    }
}
