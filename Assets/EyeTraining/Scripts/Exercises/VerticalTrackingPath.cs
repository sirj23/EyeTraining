using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class VerticalTrackingPath : ITrackingPath
    {
        private const float BottomViewportMargin = 0.22f;
        private const float TopViewportMargin = 0.26f;
        private const float TargetViewportX = 0.5f;
        private const float TargetViewportSpeed = 77f / 1080f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float bottomLimit = BottomViewportMargin + targetExtentsInViewport.y;
            float topLimit = 1f - TopViewportMargin - targetExtentsInViewport.y;
            float travelDistance = topLimit - bottomLimit;
            float viewportY = bottomLimit
                + Mathf.PingPong((float)(elapsedTime * TargetViewportSpeed), travelDistance);

            return new Vector2(TargetViewportX, viewportY);
        }
    }
}
