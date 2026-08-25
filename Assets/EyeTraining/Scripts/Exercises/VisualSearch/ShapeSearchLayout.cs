using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchLayout
    {
        public const int ItemCount = 20;
        public const float Left = 0.10f;
        public const float Right = 0.90f;
        public const float Bottom = 0.18f;
        public const float Top = 0.72f;
        public const float MinimumCenterDistanceInViewportHeight = 0.105f;

        private const int RegionCount = 4;
        private const int ItemsPerRegion = ItemCount / RegionCount;
        private const int MaximumPlacementAttempts = 600;

        private readonly IReadOnlyList<ShapeSearchLayoutItem> _items;

        private ShapeSearchLayout(IReadOnlyList<ShapeSearchLayoutItem> items)
        {
            _items = items;
        }

        public IReadOnlyList<ShapeSearchLayoutItem> Items => _items;

        public static ShapeSearchLayout Create(int seed, float aspectRatio)
        {
            ValidateAspectRatio(aspectRatio);
            var random = new System.Random(seed);
            var positions = new List<ShapeSearchLayoutItem>(ItemCount);

            for (var region = 0; region < RegionCount; region++)
            {
                for (var item = 0; item < ItemsPerRegion; item++)
                {
                    positions.Add(new ShapeSearchLayoutItem(
                        positions.Count,
                        FindPosition(region, positions, aspectRatio, random),
                        region));
                }
            }

            Shuffle(positions, random);
            var normalized = new List<ShapeSearchLayoutItem>(ItemCount);
            for (var index = 0; index < positions.Count; index++)
            {
                normalized.Add(new ShapeSearchLayoutItem(
                    index,
                    positions[index].ViewportPosition,
                    positions[index].Region));
            }

            return new ShapeSearchLayout(normalized.AsReadOnly());
        }

        public static float DistanceInViewportHeight(
            Vector2 first,
            Vector2 second,
            float aspectRatio)
        {
            ValidateAspectRatio(aspectRatio);
            float x = (first.x - second.x) * aspectRatio;
            float y = first.y - second.y;
            return (float)Math.Sqrt((x * x) + (y * y));
        }

        private static Vector2 FindPosition(
            int region,
            IReadOnlyList<ShapeSearchLayoutItem> existing,
            float aspectRatio,
            System.Random random)
        {
            GetRegionBounds(region, out float minX, out float maxX, out float minY, out float maxY);
            for (var attempt = 0; attempt < MaximumPlacementAttempts; attempt++)
            {
                var candidate = new Vector2(
                    Lerp(minX, maxX, (float)random.NextDouble()),
                    Lerp(minY, maxY, (float)random.NextDouble()));
                if (HasRequiredSeparation(candidate, existing, aspectRatio))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                $"Could not place Shape Search item in region {region}.");
        }

        private static bool HasRequiredSeparation(
            Vector2 candidate,
            IReadOnlyList<ShapeSearchLayoutItem> existing,
            float aspectRatio)
        {
            for (var index = 0; index < existing.Count; index++)
            {
                if (DistanceInViewportHeight(
                        candidate,
                        existing[index].ViewportPosition,
                        aspectRatio)
                    < MinimumCenterDistanceInViewportHeight)
                {
                    return false;
                }
            }

            return true;
        }

        private static void GetRegionBounds(
            int region,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY)
        {
            const float CenterX = (Left + Right) * 0.5f;
            const float CenterY = (Bottom + Top) * 0.5f;
            bool right = (region & 1) != 0;
            bool top = (region & 2) != 0;
            minX = right ? CenterX + 0.015f : Left;
            maxX = right ? Right : CenterX - 0.015f;
            minY = top ? CenterY + 0.015f : Bottom;
            maxY = top ? Top : CenterY - 0.015f;
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                int swap = random.Next(index + 1);
                (values[index], values[swap]) = (values[swap], values[index]);
            }
        }

        private static float Lerp(float minimum, float maximum, float value)
        {
            return minimum + ((maximum - minimum) * value);
        }

        private static void ValidateAspectRatio(float aspectRatio)
        {
            if (aspectRatio <= 0f || float.IsNaN(aspectRatio) || float.IsInfinity(aspectRatio))
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            }
        }
    }
}
