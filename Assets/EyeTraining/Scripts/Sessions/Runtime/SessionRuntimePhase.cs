namespace EyeTraining.Sessions.Runtime
{
    public enum SessionRuntimePhase
    {
        Inactive,
        Prepared,
        Preparing,
        RunningExercise,
        WaitingForContinue,
        Completing,
        Completed,
        Aborted,
        Error
    }
}
