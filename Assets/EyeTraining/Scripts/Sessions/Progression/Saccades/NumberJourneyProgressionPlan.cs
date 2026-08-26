using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.Saccades
{
    public sealed class NumberJourneyProgressionPlan
    {
        private readonly Dictionary<int, NumberJourneyLevelSettings> _settingsByLevel;

        public NumberJourneyProgressionPlan(
            IEnumerable<NumberJourneyLevelSettings> levels,
            int requiredCompletedExecutionsPerLevel)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            if (requiredCompletedExecutionsPerLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredCompletedExecutionsPerLevel));
            }

            var copy = new List<NumberJourneyLevelSettings>(levels);
            if (copy.Count == 0 || copy[0] == null || copy[0].Level != 0)
            {
                throw new ArgumentException("Number Journey progression must start at level 0.", nameof(levels));
            }

            _settingsByLevel = new Dictionary<int, NumberJourneyLevelSettings>();
            for (var index = 0; index < copy.Count; index++)
            {
                NumberJourneyLevelSettings settings = copy[index];
                if (settings == null || settings.Level != index)
                {
                    throw new ArgumentException(
                        "Number Journey levels must be non-null and sequential.",
                        nameof(levels));
                }

                _settingsByLevel.Add(settings.Level, settings);
            }

            Levels = copy.AsReadOnly();
            RequiredCompletedExecutionsPerLevel = requiredCompletedExecutionsPerLevel;
        }

        public IReadOnlyList<NumberJourneyLevelSettings> Levels { get; }

        public int RequiredCompletedExecutionsPerLevel { get; }

        public bool ContainsLevel(int level) => _settingsByLevel.ContainsKey(level);

        public NumberJourneyLevelSettings GetSettings(int level)
        {
            if (!_settingsByLevel.TryGetValue(level, out NumberJourneyLevelSettings settings))
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            return settings;
        }
    }
}
