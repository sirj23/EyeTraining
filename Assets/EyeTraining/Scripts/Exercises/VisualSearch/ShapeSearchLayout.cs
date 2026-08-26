using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchLayout
    {
        public const float Left = 0.10f;
        public const float Right = 0.90f;
        public const float Bottom = 0.18f;
        public const float Top = 0.72f;
        public const int RegionCount = 6;
        private const int MaximumPlacementAttempts = 1200;

        private readonly IReadOnlyList<ShapeSearchLayoutItem> _items;

        private ShapeSearchLayout(IReadOnlyList<ShapeSearchLayoutItem> items)
        {
            _items = items;
        }

        public IReadOnlyList<ShapeSearchLayoutItem> Items => _items;

        public static ShapeSearchLayout Create(
            int seed,
            float aspectRatio,
            int itemCount,
            float objectSizeViewportHeight)
        {
            ValidateAspectRatio(aspectRatio);
            if (itemCount <= 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
            if (objectSizeViewportHeight <= 0f) throw new ArgumentOutOfRangeException(nameof(objectSizeViewportHeight));
            var random = new System.Random(seed);
            var positions = new List<ShapeSearchLayoutItem>(itemCount);
            float minimumDistance = GetMinimumCenterDistance(itemCount, objectSizeViewportHeight);

            // Seed every logical target region, then place the remaining objects
            // freely. Regions guide target dispersion without making the board grid-like.
            for (var region = 0; region < RegionCount; region++)
            {
                positions.Add(new ShapeSearchLayoutItem(
                    positions.Count,
                    FindPosition(region, positions, aspectRatio, objectSizeViewportHeight, minimumDistance, random),
                    region));
            }

            while (positions.Count < itemCount)
            {
                Vector2 position = FindPosition(
                    -1,
                    positions,
                    aspectRatio,
                    objectSizeViewportHeight,
                    minimumDistance,
                    random);
                positions.Add(new ShapeSearchLayoutItem(
                    positions.Count,
                    position,
                    GetRegion(position)));
            }

            Shuffle(positions, random);
            var normalized = new List<ShapeSearchLayoutItem>(itemCount);
            for (var index = 0; index < positions.Count; index++)
            {
                normalized.Add(new ShapeSearchLayoutItem(
                    index,
                    positions[index].ViewportPosition,
                    positions[index].Region));
            }

            return new ShapeSearchLayout(normalized.AsReadOnly());
        }

        public static float GetMinimumCenterDistance(int itemCount, float objectSizeViewportHeight)
        {
            if (itemCount <= 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
            if (objectSizeViewportHeight <= 0f) throw new ArgumentOutOfRangeException(nameof(objectSizeViewportHeight));
            float densityDistance = 0.105f - (Math.Max(0, itemCount - 20) * 0.0025f);
            return Math.Max(objectSizeViewportHeight * 1.2f, densityDistance);
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
            float objectSizeViewportHeight,
            float minimumDistance,
            System.Random random)
        {
            float minX;
            float maxX;
            float minY;
            float maxY;
            if (region >= 0)
                GetRegionBounds(region, out minX, out maxX, out minY, out maxY);
            else
            {
                minX = Left;
                maxX = Right;
                minY = Bottom;
                maxY = Top;
            }
            float halfHeight = objectSizeViewportHeight * 0.5f;
            float halfWidth = halfHeight / aspectRatio;
            minX += halfWidth;
            maxX -= halfWidth;
            minY += halfHeight;
            maxY -= halfHeight;
            for (var attempt = 0; attempt < MaximumPlacementAttempts; attempt++)
            {
                var candidate = new Vector2(
                    Lerp(minX, maxX, (float)random.NextDouble()),
                    Lerp(minY, maxY, (float)random.NextDouble()));
                if (HasRequiredSeparation(candidate, existing, aspectRatio, minimumDistance))
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

        private static void GetRegionBounds(
            int region,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY)
        {
            const float ColumnWidth = (Right - Left) / 3f;
            const float CenterY = (Bottom + Top) * 0.5f;
            int column = region % 3;
            bool top = region >= 3;
            minX = Left + (column * ColumnWidth) + 0.008f;
            maxX = Left + ((column + 1) * ColumnWidth) - 0.008f;
            minY = top ? CenterY + 0.015f : Bottom;
            maxY = top ? Top : CenterY - 0.015f;
        }

        private static int GetRegion(Vector2 position)
        {
            int column = Math.Min(2, (int)((position.x - Left) / ((Right - Left) / 3f)));
            int row = position.y >= (Bottom + Top) * 0.5f ? 1 : 0;
            return (row * 3) + Math.Max(0, column);
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
