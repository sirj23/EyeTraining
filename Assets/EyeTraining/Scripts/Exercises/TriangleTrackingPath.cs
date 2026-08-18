using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class TriangleTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.28f;
        private const float HalfHeightInViewportHeight = 0.18f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            Vector2 leftBottom = new(
                -HalfWidthInViewportHeight * safeScale,
                -HalfHeightInViewportHeight * safeScale);
            Vector2 top = new(0f, HalfHeightInViewportHeight * safeScale);
            Vector2 rightBottom = new(
                HalfWidthInViewportHeight * safeScale,
                -HalfHeightInViewportHeight * safeScale);
            float leftSideLength = Vector2.Distance(leftBottom, top);
            float rightSideLength = Vector2.Distance(top, rightBottom);
            float bottomSideLength = Vector2.Distance(rightBottom, leftBottom);
            float perimeter = leftSideLength + rightSideLength + bottomSideLength;
            float traveledDistance = (float)(elapsedTime * LinearSpeed % perimeter);
            Vector2 localPosition;

            if (traveledDistance < leftSideLength)
            {
                localPosition = Vector2.Lerp(
                    leftBottom,
                    top,
                    traveledDistance / leftSideLength);
            }
            else
            {
                traveledDistance -= leftSideLength;

                if (traveledDistance < rightSideLength)
                {
                    localPosition = Vector2.Lerp(
                        top,
                        rightBottom,
                        traveledDistance / rightSideLength);
                }
                else
                {
                    traveledDistance -= rightSideLength;
                    localPosition = Vector2.Lerp(
                        rightBottom,
                        leftBottom,
                        traveledDistance / bottomSideLength);
                }
            }

            return ToViewport(localPosition, aspectCorrection);
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

        private static Vector2 ToViewport(Vector2 localPosition, float aspectCorrection)
        {
            return new Vector2(
                CenterViewportX + localPosition.x * aspectCorrection,
                CenterViewportY + localPosition.y);
        }
    }
}
