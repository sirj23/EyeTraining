namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public sealed class ShapeSearchProgressionState
    {
        public ShapeSearchProgressionState(ShapeSearchLevelSettings settings, int completedTowardsNextLevel, int requiredForNextLevel, bool isMaxLevel)
        {
            Settings = settings;
            CompletedTowardsNextLevel = completedTowardsNextLevel;
            RequiredForNextLevel = requiredForNextLevel;
            IsMaxLevel = isMaxLevel;
        }

        public int CurrentLevel => Settings.Level;
        public ShapeSearchLevelSettings Settings { get; }
        public int CompletedTowardsNextLevel { get; }
        public int RequiredForNextLevel { get; }
        public float Progress01 => IsMaxLevel ? 1f : (float)CompletedTowardsNextLevel / RequiredForNextLevel;
        public bool IsMaxLevel { get; }
    }
}
