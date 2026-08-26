using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeTraining.Exercises.Saccades
{
    public sealed class NumberJourneyLayout
    {
        public const int MinimumNumberCount = 9;
        public const int MaximumNumberCount = 12;
        public const float SafeLeft = 0.12f;
        public const float SafeRight = 0.88f;
        public const float SafeBottom = 0.18f;
        public const float SafeTop = 0.82f;

        private static readonly Vector2[] BasePositions =
        {
            new(0.16f, 0.72f),
            new(0.40f, 0.79f),
            new(0.72f, 0.72f),
            new(0.84f, 0.53f),
            new(0.60f, 0.57f),
            new(0.25f, 0.52f),
            new(0.15f, 0.30f),
            new(0.43f, 0.26f),
            new(0.76f, 0.31f),
            new(0.50f, 0.43f),
            new(0.31f, 0.67f),
            new(0.64f, 0.20f)
        };

        private readonly IReadOnlyList<NumberJourneyLayoutItem> _items;

        private NumberJourneyLayout(IReadOnlyList<NumberJourneyLayoutItem> items)
        {
            _items = items;
        }

        public IReadOnlyList<NumberJourneyLayoutItem> Items => _items;

        public int NumberCount => _items.Count;

        public NumberJourneyLayoutItem GetByNumber(int number)
        {
            if (number < 1 || number > NumberCount)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            return _items[number - 1];
        }

        public static NumberJourneyLayout Create(
            int seed,
            float aspectRatio,
            int numberCount)
        {
            if (aspectRatio <= 0f || float.IsNaN(aspectRatio))
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            }

            if (numberCount < MinimumNumberCount || numberCount > MaximumNumberCount)
            {
                throw new ArgumentOutOfRangeException(nameof(numberCount));
            }

            var random = new System.Random(seed);
            var items = new List<NumberJourneyLayoutItem>(numberCount);
            float minimumDistance = GetMinimumCenterDistance(numberCount);
            for (var index = 0; index < numberCount; index++)
            {
                Vector2 position = CreateSeparatedPosition(
                    BasePositions[index],
                    items,
                    aspectRatio,
                    minimumDistance,
                    random);
                float rotation = index % 3 == 1
                    ? 0f
                    : Lerp(-20f, 20f, (float)random.NextDouble());
                items.Add(new NumberJourneyLayoutItem(index + 1, position, rotation));
            }

            return new NumberJourneyLayout(items.AsReadOnly());
        }

        public static float GetMinimumCenterDistance(int numberCount)
        {
            return numberCount switch
            {
                9 => 0.18f,
                10 => 0.17f,
                11 => 0.16f,
                12 => 0.145f,
                _ => throw new ArgumentOutOfRangeException(nameof(numberCount))
            };
        }

        public static float DistanceInViewportHeight(
            Vector2 first,
            Vector2 second,
            float aspectRatio)
        {
            float horizontal = (first.x - second.x) * aspectRatio;
            float vertical = first.y - second.y;
            return (float)Math.Sqrt(
                (horizontal * horizontal) + (vertical * vertical));
        }

        private static Vector2 CreateSeparatedPosition(
            Vector2 basePosition,
            IReadOnlyList<NumberJourneyLayoutItem> existing,
            float aspectRatio,
            float minimumDistance,
            System.Random random)
        {
            for (var attempt = 0; attempt < 12; attempt++)
            {
                var jitter = new Vector2(
                    Lerp(-0.022f, 0.022f, (float)random.NextDouble()),
                    Lerp(-0.018f, 0.018f, (float)random.NextDouble()));
                Vector2 candidate = basePosition + jitter;
                if (HasRequiredSeparation(
                        candidate,
                        existing,
                        aspectRatio,
                        minimumDistance))
                {
                    return candidate;
                }
            }

            return basePosition;
        }

        private static float Lerp(float minimum, float maximum, float value)
        {
            return minimum + ((maximum - minimum) * value);
        }

        private static bool HasRequiredSeparation(
            Vector2 candidate,
            IReadOnlyList<NumberJourneyLayoutItem> existing,
            float aspectRatio,
            float minimumDistance)
        {
            for (var index = 0; index < existing.Count; index++)
            {
                if (DistanceInViewportHeight(
                        candidate,
                        existing[index].ViewportPosition,
                        aspectRatio)
                    < minimumDistance)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
