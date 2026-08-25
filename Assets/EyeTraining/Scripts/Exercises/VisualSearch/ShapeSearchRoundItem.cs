using UnityEngine;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchRoundItem
    {
        public ShapeSearchRoundItem(
            int index,
            Vector2 viewportPosition,
            int region,
            ShapeSearchShape shape,
            bool isTarget)
        {
            Index = index;
            ViewportPosition = viewportPosition;
            Region = region;
            Shape = shape;
            IsTarget = isTarget;
        }

        public int Index { get; }

        public Vector2 ViewportPosition { get; }

        public int Region { get; }

        public ShapeSearchShape Shape { get; }

        public bool IsTarget { get; }
    }
}
