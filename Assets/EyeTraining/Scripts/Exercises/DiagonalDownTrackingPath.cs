using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class DiagonalDownTrackingPath : ITrackingPath
    {
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportMargin = 0.22f;
        private const float TopViewportMargin = 0.26f;
        private const float TraversalDurationSeconds = 6.3f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            Vector2 start = new Vector2(
                HorizontalViewportMargin + targetExtentsInViewport.x,
                1f - TopViewportMargin - targetExtentsInViewport.y);
            Vector2 end = new Vector2(
                1f - HorizontalViewportMargin - targetExtentsInViewport.x,
                BottomViewportMargin + targetExtentsInViewport.y);
            float pathLength = Vector2.Distance(start, end);
            float pathSpeed = pathLength / TraversalDurationSeconds;
            float traveledDistance = Mathf.PingPong(
                (float)elapsedTime * pathSpeed,
                pathLength);
            float progress = traveledDistance / pathLength;

            return Vector2.Lerp(start, end, progress);
        }
    }
}
