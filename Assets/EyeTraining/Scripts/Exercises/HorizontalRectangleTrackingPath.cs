using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalRectangleTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.32f;
        private const float HalfHeightInViewportHeight = 0.16f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float halfHeight = HalfHeightInViewportHeight * safeScale;
            float width = halfWidth * 2f;
            float height = halfHeight * 2f;
            float perimeter = 2f * (width + height);
            float traveledDistance = (float)(elapsedTime * LinearSpeed % perimeter);
            float halfViewportWidth = halfWidth * aspectCorrection;
            float left = CenterViewportX - halfViewportWidth;
            float right = CenterViewportX + halfViewportWidth;
            float top = CenterViewportY + halfHeight;
            float bottom = CenterViewportY - halfHeight;

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

        private static float GetSafeScale(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            float horizontalSpace =
                0.5f - HorizontalViewportMargin - targetExtentsInViewport.x;
            float bottomSpace =
                CenterViewportY - BottomViewportLimit - targetExtentsInViewport.y;
            float topSpace =
                TopViewportLimit - CenterViewportY - targetExtentsInViewport.y;
            float horizontalScale =
                horizontalSpace / (HalfWidthInViewportHeight * aspectCorrection);
            float verticalScale =
                Mathf.Min(bottomSpace, topSpace) / HalfHeightInViewportHeight;

            return Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
        }
    }
}
