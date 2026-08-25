using UnityEngine;
using UnityEngine.UI;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchGraphic : MaskableGraphic
    {
        private const int CircleSegments = 48;

        [SerializeField] private ShapeSearchShape shape;

        public ShapeSearchShape Shape
        {
            get => shape;
            set
            {
                if (shape == value)
                {
                    return;
                }

                shape = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.44f;
            Vector2 center = rect.center;

            switch (shape)
            {
                case ShapeSearchShape.Circle:
                    AddCircle(vertexHelper, center, radius);
                    break;
                case ShapeSearchShape.Square:
                    AddPolygon(vertexHelper, center, radius, 4, 45f);
                    break;
                case ShapeSearchShape.Triangle:
                    AddPolygon(vertexHelper, center, radius, 3, 90f);
                    break;
                case ShapeSearchShape.Diamond:
                    AddDiamond(vertexHelper, center, radius);
                    break;
            }
        }

        private void AddCircle(VertexHelper vertexHelper, Vector2 center, float radius)
        {
            AddPolygon(vertexHelper, center, radius, CircleSegments, 90f);
        }

        private void AddPolygon(
            VertexHelper vertexHelper,
            Vector2 center,
            float radius,
            int sides,
            float startingAngleDegrees)
        {
            int centerIndex = vertexHelper.currentVertCount;
            vertexHelper.AddVert(center, color, Vector2.zero);
            for (var index = 0; index < sides; index++)
            {
                float angle = (startingAngleDegrees + (index * 360f / sides)) * Mathf.Deg2Rad;
                vertexHelper.AddVert(
                    center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius,
                    color,
                    Vector2.zero);
            }

            for (var index = 0; index < sides; index++)
            {
                vertexHelper.AddTriangle(
                    centerIndex,
                    centerIndex + 1 + index,
                    centerIndex + 1 + ((index + 1) % sides));
            }
        }

        private void AddDiamond(VertexHelper vertexHelper, Vector2 center, float radius)
        {
            int start = vertexHelper.currentVertCount;
            vertexHelper.AddVert(center + Vector2.up * radius, color, Vector2.zero);
            vertexHelper.AddVert(center + Vector2.right * radius * 0.78f, color, Vector2.zero);
            vertexHelper.AddVert(center + Vector2.down * radius, color, Vector2.zero);
            vertexHelper.AddVert(center + Vector2.left * radius * 0.78f, color, Vector2.zero);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
            vertexHelper.AddTriangle(start, start + 2, start + 3);
        }
    }
}
