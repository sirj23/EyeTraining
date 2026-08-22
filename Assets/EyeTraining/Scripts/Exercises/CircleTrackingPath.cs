using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class CircleTrackingPath : IClosedTrackingPath
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
            float fullCycleLength = Mathf.PI * 2f * radius;
            float traveledDistance =
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % fullCycleLength);
            float angle = traveledDistance / radius;
            float viewportX = TrackingTrainingArea.Center.x + radiusX * Mathf.Sin(angle);
            float viewportY = TrackingTrainingArea.Center.y + radius * Mathf.Cos(angle);

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

            return Mathf.PI * 2f * RadiusInViewportHeight * safeScale;
        }
    }
}
