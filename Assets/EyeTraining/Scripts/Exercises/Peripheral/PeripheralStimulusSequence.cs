using System;
using System.Collections.Generic;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class PeripheralStimulusSequence
    {
        public const int TrialCount = 12;
        public const float MinimumDelay = 1.2f;
        public const float MaximumDelay = 2.2f;
        public const float InitialMinimumDelay = 1.5f;
        public const float InitialMaximumDelay = 2.0f;

        private readonly IReadOnlyList<PeripheralTrial> _trials;

        private PeripheralStimulusSequence(IReadOnlyList<PeripheralTrial> trials)
        {
            _trials = trials;
        }

        public IReadOnlyList<PeripheralTrial> Trials => _trials;

        public static PeripheralStimulusSequence Create(
            int seed,
            bool fixedDirection = false,
            PeripheralDirection direction = PeripheralDirection.Up)
        {
            if (!Enum.IsDefined(typeof(PeripheralDirection), direction))
                throw new ArgumentOutOfRangeException(nameof(direction));

            var random = new System.Random(seed);
            var directions = fixedDirection
                ? CreateFixedDirections(direction)
                : CreateBalancedDirections(random);
            var trials = new List<PeripheralTrial>(TrialCount);
            for (var index = 0; index < TrialCount; index++)
            {
                float minimum = index == 0 ? InitialMinimumDelay : MinimumDelay;
                float maximum = index == 0 ? InitialMaximumDelay : MaximumDelay;
                float delay = minimum + ((maximum - minimum) * (float)random.NextDouble());
                trials.Add(new PeripheralTrial(directions[index], delay));
            }

            return new PeripheralStimulusSequence(trials.AsReadOnly());
        }

        private static List<PeripheralDirection> CreateBalancedDirections(System.Random random)
        {
            var pool = new List<PeripheralDirection>(TrialCount);
            foreach (PeripheralDirection direction in Enum.GetValues(typeof(PeripheralDirection)))
                pool.Add(direction);
            for (var index = pool.Count; index < TrialCount; index++)
                pool.Add((PeripheralDirection)random.Next(8));

            for (var attempt = 0; attempt < 100; attempt++)
            {
                Shuffle(pool, random);
                if (!HasAdjacentDuplicate(pool)) return pool;
            }

            throw new InvalidOperationException("Could not create a valid peripheral sequence.");
        }

        private static List<PeripheralDirection> CreateFixedDirections(PeripheralDirection direction)
        {
            var result = new List<PeripheralDirection>(TrialCount);
            for (var index = 0; index < TrialCount; index++) result.Add(direction);
            return result;
        }

        private static bool HasAdjacentDuplicate(IReadOnlyList<PeripheralDirection> values)
        {
            for (var index = 1; index < values.Count; index++)
                if (values[index] == values[index - 1]) return true;
            return false;
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }
    }
}
