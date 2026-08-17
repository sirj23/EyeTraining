using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalTrackingPath : ITrackingPath
    {
        private const float HorizontalViewportMargin = 0.12f;
        private const float TargetViewportY = 0.48f;
        private const float TargetViewportSpeed = 220f / 1920f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float leftLimit = HorizontalViewportMargin + targetExtentsInViewport.x;
            float rightLimit = 1f - HorizontalViewportMargin - targetExtentsInViewport.x;
            float travelDistance = rightLimit - leftLimit;
            float viewportX = leftLimit
                + Mathf.PingPong((float)(elapsedTime * TargetViewportSpeed), travelDistance);

            return new Vector2(viewportX, TargetViewportY);
        }
    }
}
