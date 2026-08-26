using UnityEngine;
using UnityEngine.UI;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class PeripheralMarkerGraphic : MaskableGraphic
    {
        private const int SegmentCount = 64;

        [SerializeField] private bool fixationMarker;

        public bool FixationMarker
        {
            get => fixationMarker;
            set
            {
                if (fixationMarker == value) return;
                fixationMarker = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
            if (radius <= 0f) return;

            if (fixationMarker)
            {
                AddRing(vertexHelper, radius, radius * 0.64f);
                AddDisc(vertexHelper, radius * 0.20f);
            }
            else
            {
                AddDisc(vertexHelper, radius);
            }
        }

        private void AddDisc(VertexHelper vertexHelper, float radius)
        {
            int center = vertexHelper.currentVertCount;
            vertexHelper.AddVert(Vector3.zero, color, Vector2.zero);
            for (var index = 0; index < SegmentCount; index++)
            {
                float angle = index * Mathf.PI * 2f / SegmentCount;
                vertexHelper.AddVert(
                    new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius),
                    color,
                    Vector2.zero);
            }

            for (var index = 0; index < SegmentCount; index++)
                vertexHelper.AddTriangle(center, center + 1 + index, center + 1 + ((index + 1) % SegmentCount));
        }

        private void AddRing(VertexHelper vertexHelper, float outerRadius, float innerRadius)
        {
            int first = vertexHelper.currentVertCount;
            for (var index = 0; index < SegmentCount; index++)
            {
                float angle = index * Mathf.PI * 2f / SegmentCount;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                vertexHelper.AddVert(new Vector3(x * outerRadius, y * outerRadius), color, Vector2.zero);
                vertexHelper.AddVert(new Vector3(x * innerRadius, y * innerRadius), color, Vector2.zero);
            }

            for (var index = 0; index < SegmentCount; index++)
            {
                int next = (index + 1) % SegmentCount;
                int outer = first + (index * 2);
                int inner = outer + 1;
                int nextOuter = first + (next * 2);
                int nextInner = nextOuter + 1;
                vertexHelper.AddTriangle(outer, nextOuter, inner);
                vertexHelper.AddTriangle(nextOuter, nextInner, inner);
            }
        }
    }
}
