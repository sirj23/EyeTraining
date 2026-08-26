using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public sealed class ShapeSearchProgressionService
    {
        private readonly ShapeSearchProgressionPlan _plan;

        public ShapeSearchProgressionService(ShapeSearchProgressionPlan plan)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public ShapeSearchLevelSettings GetSettings(int level) => _plan.GetSettings(level);

        public ShapeSearchProgressionState GetState(ShapeSearchProgressionHistory history, int currentSessionNumber)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));
            if (currentSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));

            var level = 0;
            var completedTowardsNext = 0;
            foreach (ShapeSearchProgressionEntry entry in history.Entries)
            {
                if (entry.CompletedSessionNumber >= currentSessionNumber)
                    throw new ArgumentException("Progression history must contain only earlier completed sessions.", nameof(history));
                if (!_plan.ContainsLevel(entry.AppliedLevel))
                    throw new ArgumentException($"History contains unknown Shape Search level {entry.AppliedLevel}.", nameof(history));
                if (entry.AppliedLevel != level)
                    throw new InvalidOperationException($"Shape Search history applies level {entry.AppliedLevel}, but level {level} was expected in session {entry.CompletedSessionNumber}.");
                if (entry.CompletionStatus == ExerciseCompletionStatus.Interrupted) continue;
                if (level == _plan.Levels.Count - 1) continue;
                completedTowardsNext++;
                if (completedTowardsNext == _plan.RequiredCompletedExecutionsPerLevel)
                {
                    level++;
                    completedTowardsNext = 0;
                }
            }

            bool isMax = level == _plan.Levels.Count - 1;
            return new ShapeSearchProgressionState(
                _plan.GetSettings(level),
                isMax ? 0 : completedTowardsNext,
                isMax ? 0 : _plan.RequiredCompletedExecutionsPerLevel,
                isMax);
        }
    }
}
