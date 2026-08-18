using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class LowerSemicircleTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float RadiusInViewportHeight = 0.18f;
        private const float TraverseDurationSeconds = 5f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float radiusX = RadiusInViewportHeight * aspectCorrection;
            float progress = Mathf.PingPong(
                (float)elapsedTime / TraverseDurationSeconds,
                1f);
            float angle = Mathf.PI * (1f + progress);
            float viewportX = CenterViewportX + radiusX * Mathf.Cos(angle);
            float viewportY = CenterViewportY + RadiusInViewportHeight * Mathf.Sin(angle);

            return new Vector2(viewportX, viewportY);
        }
    }
}
