using UnityEngine;
using UnityEngine.Rendering;

namespace EyeTraining.Exercises
{
    public sealed class TrackingPathRenderer
    {
        private const int SampleCount = 256;
        private const float LineWidthInViewportHeight = 0.002f;

        private static readonly Color BaseColor = new(0.40f, 0.52f, 0.64f, 1f);

        private readonly Camera camera;
        private readonly LineRenderer lineRenderer;
        private ITrackingPath path;
        private Vector2 targetExtentsInViewport;
        private TrackingPathVisibility visibility;
        private float targetPlaneZ;
        private float lastOrthographicSize = -1f;
        private float lastTargetPlaneZ = float.NaN;
        private int lastPixelWidth = -1;
        private int lastPixelHeight = -1;
        private bool geometryDirty;

        public TrackingPathRenderer(
            Transform parent,
            Camera camera,
            Material material,
            int sortingLayerId,
            int sortingOrder)
        {
            this.camera = camera;

            GameObject lineObject = new("Tracking Path");
            lineObject.transform.SetParent(parent, false);
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startColor = BaseColor;
            lineRenderer.endColor = BaseColor;
            lineRenderer.numCapVertices = 0;
            lineRenderer.numCornerVertices = 0;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.lightProbeUsage = LightProbeUsage.Off;
            lineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            lineRenderer.sharedMaterial = material;
            lineRenderer.sortingLayerID = sortingLayerId;
            lineRenderer.sortingOrder = sortingOrder;
        }

        public void Show(
            ITrackingPath path,
            Vector2 targetExtentsInViewport,
            TrackingPathVisibility visibility,
            float targetPlaneZ)
        {
            this.path = path;
            this.targetExtentsInViewport = targetExtentsInViewport;
            this.visibility = visibility;
            this.targetPlaneZ = targetPlaneZ;
            geometryDirty = true;

            ApplyVisibility();
            UpdateIfNeeded();
        }

        public void Hide()
        {
            lineRenderer.enabled = false;
        }

        public void UpdateIfNeeded()
        {
            if (visibility == TrackingPathVisibility.Hidden || path == null)
            {
                lineRenderer.enabled = false;
                return;
            }

            bool viewChanged =
                lastPixelWidth != camera.pixelWidth ||
                lastPixelHeight != camera.pixelHeight ||
                !Mathf.Approximately(lastOrthographicSize, camera.orthographicSize) ||
                !Mathf.Approximately(lastTargetPlaneZ, targetPlaneZ);

            if (!geometryDirty && !viewChanged)
            {
                return;
            }

            RebuildLine();
            geometryDirty = false;
            lastPixelWidth = camera.pixelWidth;
            lastPixelHeight = camera.pixelHeight;
            lastOrthographicSize = camera.orthographicSize;
            lastTargetPlaneZ = targetPlaneZ;
        }

        private void RebuildLine()
        {
            bool isClosed = path is IClosedTrackingPath;
            float fullCycleLength = path.GetFullCycleLength(targetExtentsInViewport);
            float drawableLength = isClosed ? fullCycleLength : fullCycleLength * 0.5f;
            int divisor = isClosed ? SampleCount : SampleCount - 1;

            lineRenderer.loop = isClosed;
            lineRenderer.positionCount = SampleCount;

            for (int index = 0; index < SampleCount; index++)
            {
                float distance = drawableLength * index / divisor;
                double elapsedTime = distance / TrackingMotionSettings.LinearSpeed;
                Vector2 viewportPosition = path.Evaluate(elapsedTime, targetExtentsInViewport);
                lineRenderer.SetPosition(index, ViewportToWorld(viewportPosition));
            }

            float visibleWorldHeight = camera.orthographicSize * 2f;
            float lineWidth = visibleWorldHeight * LineWidthInViewportHeight;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }

        private void ApplyVisibility()
        {
            float alpha = visibility switch
            {
                TrackingPathVisibility.VerySubtle => 0.10f,
                TrackingPathVisibility.Subtle => 0.21f,
                TrackingPathVisibility.Clear => 0.34f,
                _ => 0f
            };
            Color color = new(BaseColor.r, BaseColor.g, BaseColor.b, alpha);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.enabled = visibility != TrackingPathVisibility.Hidden;
        }

        private Vector3 ViewportToWorld(Vector2 viewportPosition)
        {
            float distanceFromCamera = targetPlaneZ - camera.transform.position.z;
            Vector3 position = camera.ViewportToWorldPoint(
                new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera));
            position.z = targetPlaneZ;
            return position;
        }
    }
}
