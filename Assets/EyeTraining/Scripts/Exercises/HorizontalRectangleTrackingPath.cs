using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalRectangleTrackingPath : ITrackingPath
    {
        private const float HorizontalCoverage = 0.88f;
        private const float HalfWidthInViewportHeight = 0.62f;
        private const float HalfHeightInViewportHeight = 0.19f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float halfHeight = HalfHeightInViewportHeight * safeScale;
            float width = halfWidth * 2f;
            float height = halfHeight * 2f;
            float perimeter = 2f * (width + height);
            float traveledDistance = (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % perimeter);
            float halfViewportWidth = halfWidth * aspectCorrection;
            float left = TrackingTrainingArea.Center.x - halfViewportWidth;
            float right = TrackingTrainingArea.Center.x + halfViewportWidth;
            float top = TrackingTrainingArea.Center.y + halfHeight;
            float bottom = TrackingTrainingArea.Center.y - halfHeight;

            if (traveledDistance < width)
            {
                return new Vector2(
                    left + traveledDistance * aspectCorrection,
                    top);
            }

            traveledDistance -= width;

            if (traveledDistance < height)
            {
                return new Vector2(right, top - traveledDistance);
            }

            traveledDistance -= height;

            if (traveledDistance < width)
            {
                return new Vector2(
                    right - traveledDistance * aspectCorrection,
                    bottom);
            }

            traveledDistance -= width;
            return new Vector2(left, bottom + traveledDistance);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float width = HalfWidthInViewportHeight * safeScale * 2f;
            float height = HalfHeightInViewportHeight * safeScale * 2f;

            return 2f * (width + height);
        }

        private static float GetSafeScale(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            float availableHalfWidth =
                (TrackingTrainingArea.Center.x
                    - TrackingTrainingArea.GetCenterLeft(targetExtentsInViewport))
                / aspectCorrection;
            float horizontalScale =
                availableHalfWidth * HorizontalCoverage / HalfWidthInViewportHeight;
            float verticalScale = TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                0f,
                HalfHeightInViewportHeight);

            return Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
        }
    }
}
