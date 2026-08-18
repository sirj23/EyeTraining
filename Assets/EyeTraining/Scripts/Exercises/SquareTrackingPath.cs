using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class SquareTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfSideInViewportHeight = 0.18f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float halfSide = GetSafeScale(targetExtentsInViewport, aspectCorrection)
                * HalfSideInViewportHeight;
            float sideLength = halfSide * 2f;
            float perimeter = sideLength * 4f;
            float traveledDistance = (float)(elapsedTime * LinearSpeed % perimeter);
            float halfViewportWidth = halfSide * aspectCorrection;
            float left = CenterViewportX - halfViewportWidth;
            float right = CenterViewportX + halfViewportWidth;
            float top = CenterViewportY + halfSide;
            float bottom = CenterViewportY - halfSide;

            if (traveledDistance < sideLength)
            {
                return new Vector2(
                    left + traveledDistance * aspectCorrection,
                    top);
            }

            traveledDistance -= sideLength;

            if (traveledDistance < sideLength)
            {
                return new Vector2(right, top - traveledDistance);
            }

            traveledDistance -= sideLength;

            if (traveledDistance < sideLength)
            {
                return new Vector2(
                    right - traveledDistance * aspectCorrection,
                    bottom);
            }

            traveledDistance -= sideLength;
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
                horizontalSpace / (HalfSideInViewportHeight * aspectCorrection);
            float verticalScale = Mathf.Min(bottomSpace, topSpace) / HalfSideInViewportHeight;

            return Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
        }
    }
}
