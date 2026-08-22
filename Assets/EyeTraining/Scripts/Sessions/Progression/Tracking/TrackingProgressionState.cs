namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionState
    {
        internal TrackingProgressionState(
            TrackingExerciseParameters parameters,
            int completedTowardsNextLevel,
            int requiredForNextLevel,
            float progress01,
            bool isMaxLevel)
        {
            Parameters = parameters;
            CurrentLevel = parameters.Level;
            CompletedTowardsNextLevel = completedTowardsNextLevel;
            RequiredForNextLevel = requiredForNextLevel;
            Progress01 = progress01;
            IsMaxLevel = isMaxLevel;
        }

        public int CurrentLevel { get; }

        public int CompletedTowardsNextLevel { get; }

        public int RequiredForNextLevel { get; }

        public float Progress01 { get; }

        public bool IsMaxLevel { get; }

        public TrackingExerciseParameters Parameters { get; }
    }
}
