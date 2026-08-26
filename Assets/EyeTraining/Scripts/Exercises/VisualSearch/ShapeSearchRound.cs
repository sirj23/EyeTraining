using System;
using System.Collections.Generic;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchRound
    {
        private readonly IReadOnlyList<ShapeSearchRoundItem> _items;

        private ShapeSearchRound(
            ShapeSearchShape targetShape,
            int targetCount,
            IReadOnlyList<ShapeSearchRoundItem> items)
        {
            TargetShape = targetShape;
            TargetCount = targetCount;
            _items = items;
        }

        public ShapeSearchShape TargetShape { get; }
        public int TargetCount { get; }

        public IReadOnlyList<ShapeSearchRoundItem> Items => _items;

        public static ShapeSearchRound Create(int seed, ShapeSearchLayout layout, int targetCount)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (targetCount <= 0 || targetCount > ShapeSearchLayout.RegionCount || targetCount >= layout.Items.Count)
                throw new ArgumentOutOfRangeException(nameof(targetCount));

            var random = new System.Random(unchecked((seed * 486187739) ^ 0x35B193));
            var shapes = (ShapeSearchShape[])Enum.GetValues(typeof(ShapeSearchShape));
            ShapeSearchShape targetShape = shapes[random.Next(shapes.Length)];
            var targetIndices = SelectTargetIndices(layout, targetCount, random);
            var distractors = CreateBalancedDistractors(targetShape, layout.Items.Count - targetCount, random);
            var items = new List<ShapeSearchRoundItem>(layout.Items.Count);
            var distractorIndex = 0;

            for (var index = 0; index < layout.Items.Count; index++)
            {
                ShapeSearchLayoutItem layoutItem = layout.Items[index];
                bool isTarget = targetIndices.Contains(index);
                ShapeSearchShape shape = isTarget
                    ? targetShape
                    : distractors[distractorIndex++];
                items.Add(new ShapeSearchRoundItem(
                    index,
                    layoutItem.ViewportPosition,
                    layoutItem.Region,
                    shape,
                    isTarget));
            }

            return new ShapeSearchRound(targetShape, targetCount, items.AsReadOnly());
        }

        private static HashSet<int> SelectTargetIndices(
            ShapeSearchLayout layout,
            int targetCount,
            System.Random random)
        {
            var targets = new HashSet<int>();
            var regions = new List<int>();
            for (var region = 0; region < ShapeSearchLayout.RegionCount; region++) regions.Add(region);
            Shuffle(regions, random);
            for (var selectedRegion = 0; selectedRegion < targetCount; selectedRegion++)
            {
                int region = regions[selectedRegion];
                var candidates = new List<int>();
                for (var index = 0; index < layout.Items.Count; index++)
                {
                    if (layout.Items[index].Region == region)
                    {
                        candidates.Add(index);
                    }
                }

                if (candidates.Count == 0)
                {
                    throw new InvalidOperationException($"Layout has no item in region {region}.");
                }

                targets.Add(candidates[random.Next(candidates.Count)]);
            }

            return targets;
        }

        private static List<ShapeSearchShape> CreateBalancedDistractors(
            ShapeSearchShape targetShape,
            int distractorCount,
            System.Random random)
        {
            var available = new List<ShapeSearchShape>();
            foreach (ShapeSearchShape shape in Enum.GetValues(typeof(ShapeSearchShape)))
            {
                if (shape != targetShape)
                {
                    available.Add(shape);
                }
            }

            Shuffle(available, random);
            var result = new List<ShapeSearchShape>(distractorCount);
            for (var index = 0; index < distractorCount; index++)
            {
                result.Add(available[index % available.Count]);
            }

            Shuffle(result, random);
            return result;
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                int swap = random.Next(index + 1);
                (values[index], values[swap]) = (values[swap], values[index]);
            }
        }
    }
}
