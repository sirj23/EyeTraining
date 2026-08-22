using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class LowerSemicircleTrackingPath : ITrackingPath
    {
        private const float RadiusInViewportHeight = 0.18f;
        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                RadiusInViewportHeight,
                RadiusInViewportHeight);
            float radius = RadiusInViewportHeight * safeScale;
            float radiusX = radius * aspectCorrection;
            float oneWayLength = Mathf.PI * radius;
            float progress = Mathf.PingPong(
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed),
                oneWayLength) / oneWayLength;
            float angle = Mathf.PI * (1f + progress);
            float viewportX = TrackingTrainingArea.Center.x + radiusX * Mathf.Cos(angle);
            float viewportY = TrackingTrainingArea.Center.y + radius * Mathf.Sin(angle);

            return new Vector2(viewportX, viewportY);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                RadiusInViewportHeight,
                RadiusInViewportHeight);

            return Mathf.PI * RadiusInViewportHeight * safeScale * 2f;
        }
    }
}
