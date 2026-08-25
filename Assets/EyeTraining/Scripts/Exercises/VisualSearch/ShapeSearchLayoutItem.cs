using UnityEngine;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchLayoutItem
    {
        public ShapeSearchLayoutItem(int index, Vector2 viewportPosition, int region)
        {
            Index = index;
            ViewportPosition = viewportPosition;
            Region = region;
        }

        public int Index { get; }

        public Vector2 ViewportPosition { get; }

        public int Region { get; }
    }
}
