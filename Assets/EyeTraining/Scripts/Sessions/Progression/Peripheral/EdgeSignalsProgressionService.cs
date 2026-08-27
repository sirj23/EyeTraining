using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsProgressionService
    {
        private readonly EdgeSignalsProgressionPlan plan;
        public EdgeSignalsProgressionService(EdgeSignalsProgressionPlan plan) => this.plan = plan ?? throw new ArgumentNullException(nameof(plan));
        public EdgeSignalsLevelSettings GetSettings(int level) => plan.GetSettings(level);
        public EdgeSignalsProgressionState GetState(EdgeSignalsProgressionHistory history, int currentSessionNumber)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));
            if (currentSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            var level = 0; var completed = 0;
            foreach (var entry in history.Entries)
            {
                if (entry.CompletedSessionNumber >= currentSessionNumber) throw new ArgumentException("History must contain earlier sessions only.", nameof(history));
                if (!plan.ContainsLevel(entry.AppliedLevel)) throw new ArgumentException("History contains an unknown level.", nameof(history));
                if (entry.AppliedLevel != level) throw new InvalidOperationException("Peripheral progression history is inconsistent.");
                if (entry.CompletionStatus == ExerciseCompletionStatus.Interrupted || level == plan.Levels.Count - 1) continue;
                if (++completed == plan.RequiredCompletedExecutionsPerLevel) { level++; completed = 0; }
            }
            bool max = level == plan.Levels.Count - 1;
            return new EdgeSignalsProgressionState(plan.GetSettings(level), max ? 0 : completed,
                max ? 0 : plan.RequiredCompletedExecutionsPerLevel, max);
        }
    }
}
