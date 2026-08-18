using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class CircleTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float RadiusInViewportHeight = 0.18f;
        private const float RevolutionDurationSeconds = 10f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float radiusX = RadiusInViewportHeight * aspectCorrection;
            float angle = (float)(elapsedTime * Mathf.PI * 2f / RevolutionDurationSeconds);
            float viewportX = CenterViewportX + radiusX * Mathf.Sin(angle);
            float viewportY = CenterViewportY + RadiusInViewportHeight * Mathf.Cos(angle);

            return new Vector2(viewportX, viewportY);
        }
    }
}
