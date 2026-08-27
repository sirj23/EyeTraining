using System;
using System.Collections.Generic;
using System.Linq;

namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsProgressionPlan
    {
        private readonly IReadOnlyList<EdgeSignalsLevelSettings> levels;
        public EdgeSignalsProgressionPlan(IEnumerable<EdgeSignalsLevelSettings> levels, int required)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            var copy = levels.ToArray();
            if (copy.Length == 0 || required <= 0) throw new ArgumentException("Progression plan is invalid.");
            for (var i = 0; i < copy.Length; i++)
                if (copy[i] == null || copy[i].Level != i) throw new ArgumentException("Levels must be contiguous from zero.");
            this.levels = Array.AsReadOnly(copy);
            RequiredCompletedExecutionsPerLevel = required;
        }
        public IReadOnlyList<EdgeSignalsLevelSettings> Levels => levels;
        public int RequiredCompletedExecutionsPerLevel { get; }
        public bool ContainsLevel(int level) => level >= 0 && level < levels.Count;
        public EdgeSignalsLevelSettings GetSettings(int level) => ContainsLevel(level)
            ? levels[level] : throw new ArgumentOutOfRangeException(nameof(level));
    }
}
