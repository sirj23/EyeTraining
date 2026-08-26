using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public sealed class ShapeSearchProgressionPlan
    {
        private readonly Dictionary<int, ShapeSearchLevelSettings> _settingsByLevel;

        public ShapeSearchProgressionPlan(
            IEnumerable<ShapeSearchLevelSettings> levels,
            int requiredCompletedExecutionsPerLevel)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            if (requiredCompletedExecutionsPerLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredCompletedExecutionsPerLevel));

            var copy = new List<ShapeSearchLevelSettings>(levels);
            if (copy.Count == 0 || copy[0] == null || copy[0].Level != 0)
                throw new ArgumentException("Shape Search progression must start at level 0.", nameof(levels));

            _settingsByLevel = new Dictionary<int, ShapeSearchLevelSettings>();
            for (var index = 0; index < copy.Count; index++)
            {
                ShapeSearchLevelSettings settings = copy[index];
                if (settings == null || settings.Level != index)
                    throw new ArgumentException("Shape Search levels must be non-null and sequential.", nameof(levels));
                _settingsByLevel.Add(settings.Level, settings);
            }

            Levels = copy.AsReadOnly();
            RequiredCompletedExecutionsPerLevel = requiredCompletedExecutionsPerLevel;
        }

        public IReadOnlyList<ShapeSearchLevelSettings> Levels { get; }
        public int RequiredCompletedExecutionsPerLevel { get; }
        public bool ContainsLevel(int level) => _settingsByLevel.ContainsKey(level);

        public ShapeSearchLevelSettings GetSettings(int level)
        {
            if (!_settingsByLevel.TryGetValue(level, out ShapeSearchLevelSettings settings))
                throw new ArgumentOutOfRangeException(nameof(level));
            return settings;
        }
    }
}
