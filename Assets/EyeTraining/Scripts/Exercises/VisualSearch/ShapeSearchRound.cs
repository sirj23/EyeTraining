using System;
using System.Collections.Generic;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchRound
    {
        public const int TargetCount = 4;
        public const int DistractorCount = ShapeSearchLayout.ItemCount - TargetCount;

        private readonly IReadOnlyList<ShapeSearchRoundItem> _items;

        private ShapeSearchRound(
            ShapeSearchShape targetShape,
            IReadOnlyList<ShapeSearchRoundItem> items)
        {
            TargetShape = targetShape;
            _items = items;
        }

        public ShapeSearchShape TargetShape { get; }

        public IReadOnlyList<ShapeSearchRoundItem> Items => _items;

        public static ShapeSearchRound Create(int seed, ShapeSearchLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (layout.Items.Count != ShapeSearchLayout.ItemCount)
            {
                throw new ArgumentException("Shape Search layout must contain 20 items.", nameof(layout));
            }

            var random = new System.Random(unchecked((seed * 486187739) ^ 0x35B193));
            var shapes = (ShapeSearchShape[])Enum.GetValues(typeof(ShapeSearchShape));
            ShapeSearchShape targetShape = shapes[random.Next(shapes.Length)];
            var targetIndices = SelectTargetIndices(layout, random);
            var distractors = CreateBalancedDistractors(targetShape, random);
            var items = new List<ShapeSearchRoundItem>(ShapeSearchLayout.ItemCount);
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

            return new ShapeSearchRound(targetShape, items.AsReadOnly());
        }

        private static HashSet<int> SelectTargetIndices(
            ShapeSearchLayout layout,
            System.Random random)
        {
            var targets = new HashSet<int>();
            for (var region = 0; region < TargetCount; region++)
            {
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
            var result = new List<ShapeSearchShape>(DistractorCount);
            for (var index = 0; index < DistractorCount; index++)
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
