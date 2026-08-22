using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.Tracking
{
    public sealed class TrackingProgressionPlan
    {
        private readonly Dictionary<int, int> _indexByLevel;

        public TrackingProgressionPlan(
            IEnumerable<TrackingProgressionLevelDefinition> levels,
            int requiredCompletedExecutionsPerLevel)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            if (requiredCompletedExecutionsPerLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCompletedExecutionsPerLevel));
            }

            var copy = new List<TrackingProgressionLevelDefinition>(levels);
            if (copy.Count == 0)
            {
                throw new ArgumentException("Progression plan must contain at least one level.", nameof(levels));
            }

            if (copy[0] == null || copy[0].Level != 0)
            {
                throw new ArgumentException("The first progression level must have id 0.", nameof(levels));
            }

            _indexByLevel = new Dictionary<int, int>();
            for (var index = 0; index < copy.Count; index++)
            {
                TrackingProgressionLevelDefinition definition = copy[index];
                if (definition == null)
                {
                    throw new ArgumentException("Progression plan cannot contain null levels.", nameof(levels));
                }

                if (_indexByLevel.ContainsKey(definition.Level))
                {
                    throw new ArgumentException("Progression level ids must be unique.", nameof(levels));
                }

                _indexByLevel.Add(definition.Level, index);
            }

            Levels = copy.AsReadOnly();
            RequiredCompletedExecutionsPerLevel = requiredCompletedExecutionsPerLevel;
        }

        public IReadOnlyList<TrackingProgressionLevelDefinition> Levels { get; }

        public int RequiredCompletedExecutionsPerLevel { get; }

        public bool ContainsLevel(int level)
        {
            return _indexByLevel.ContainsKey(level);
        }

        public int GetIndex(int level)
        {
            if (!_indexByLevel.TryGetValue(level, out int index))
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Unknown progression level.");
            }

            return index;
        }
    }
}
