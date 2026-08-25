using System;
using System.Collections.Generic;

namespace EyeTraining.Exercises.Saccades
{
    public sealed class NumberJourneySequence
    {
        public const int Length = 5;
        public const float MinimumPreferredJumpInViewportHeight = 0.38f;

        private readonly IReadOnlyList<int> _numbers;

        private NumberJourneySequence(IReadOnlyList<int> numbers)
        {
            _numbers = numbers;
        }

        public IReadOnlyList<int> Numbers => _numbers;

        public static NumberJourneySequence Create(
            int seed,
            NumberJourneyLayout layout,
            float aspectRatio)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (aspectRatio <= 0f || float.IsNaN(aspectRatio))
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            }

            var random = new System.Random(unchecked(seed * 397) ^ 0x51A7C3);
            var available = new List<int>(NumberJourneyLayout.NumberCount);
            for (var number = 1; number <= NumberJourneyLayout.NumberCount; number++)
            {
                available.Add(number);
            }

            var selected = new List<int>(Length);
            int firstIndex = random.Next(available.Count);
            selected.Add(available[firstIndex]);
            available.RemoveAt(firstIndex);

            while (selected.Count < Length)
            {
                int previous = selected[selected.Count - 1];
                available.Sort((left, right) => CompareByDistance(
                    previous,
                    left,
                    right,
                    layout,
                    aspectRatio));

                int preferredCount = CountPreferred(
                    previous,
                    available,
                    layout,
                    aspectRatio);
                int candidatePool = Math.Max(1, Math.Min(3, preferredCount));
                int selectedIndex = random.Next(candidatePool);
                selected.Add(available[selectedIndex]);
                available.RemoveAt(selectedIndex);
            }

            return new NumberJourneySequence(selected.AsReadOnly());
        }

        private static int CompareByDistance(
            int previous,
            int left,
            int right,
            NumberJourneyLayout layout,
            float aspectRatio)
        {
            float leftDistance = GetDistance(previous, left, layout, aspectRatio);
            float rightDistance = GetDistance(previous, right, layout, aspectRatio);
            int distanceComparison = rightDistance.CompareTo(leftDistance);
            return distanceComparison != 0 ? distanceComparison : left.CompareTo(right);
        }

        private static int CountPreferred(
            int previous,
            IReadOnlyList<int> available,
            NumberJourneyLayout layout,
            float aspectRatio)
        {
            var count = 0;
            for (var index = 0; index < available.Count; index++)
            {
                if (GetDistance(previous, available[index], layout, aspectRatio)
                    >= MinimumPreferredJumpInViewportHeight)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        private static float GetDistance(
            int first,
            int second,
            NumberJourneyLayout layout,
            float aspectRatio)
        {
            return NumberJourneyLayout.DistanceInViewportHeight(
                layout.GetByNumber(first).ViewportPosition,
                layout.GetByNumber(second).ViewportPosition,
                aspectRatio);
        }
    }
}
