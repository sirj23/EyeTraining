namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsProgressionState
    {
        public EdgeSignalsProgressionState(EdgeSignalsLevelSettings settings, int completed, int required, bool isMax)
        { Settings = settings; CompletedTowardsNextLevel = completed; RequiredForNextLevel = required; IsMaxLevel = isMax; }
        public int CurrentLevel => Settings.Level;
        public EdgeSignalsLevelSettings Settings { get; }
        public int CompletedTowardsNextLevel { get; }
        public int RequiredForNextLevel { get; }
        public bool IsMaxLevel { get; }
        public float Progress01 => IsMaxLevel ? 1f : (float)CompletedTowardsNextLevel / RequiredForNextLevel;
    }
}
