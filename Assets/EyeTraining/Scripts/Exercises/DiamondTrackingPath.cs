using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class DiamondTrackingPath : ITrackingPath
    {
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.24f;
        private const float HalfHeightInViewportHeight = 0.18f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            Vector2 top = new(0f, HalfHeightInViewportHeight * safeScale);
            Vector2 right = new(HalfWidthInViewportHeight * safeScale, 0f);
            Vector2 bottom = new(0f, -HalfHeightInViewportHeight * safeScale);
            Vector2 left = new(-HalfWidthInViewportHeight * safeScale, 0f);
            float topRightLength = Vector2.Distance(top, right);
            float rightBottomLength = Vector2.Distance(right, bottom);
            float bottomLeftLength = Vector2.Distance(bottom, left);
            float leftTopLength = Vector2.Distance(left, top);
            float perimeter =
                topRightLength + rightBottomLength + bottomLeftLength + leftTopLength;
            float traveledDistance = (float)(elapsedTime * LinearSpeed % perimeter);
            Vector2 localPosition;

            if (traveledDistance < topRightLength)
            {
                localPosition = Vector2.Lerp(
                    top,
                    right,
                    traveledDistance / topRightLength);
            }
            else
            {
                traveledDistance -= topRightLength;

                if (traveledDistance < rightBottomLength)
                {
                    localPosition = Vector2.Lerp(
                        right,
                        bottom,
                        traveledDistance / rightBottomLength);
                }
                else
                {
                    traveledDistance -= rightBottomLength;

                    if (traveledDistance < bottomLeftLength)
                    {
                        localPosition = Vector2.Lerp(
                            bottom,
                            left,
                            traveledDistance / bottomLeftLength);
                    }
                    else
                    {
                        traveledDistance -= bottomLeftLength;
                        localPosition = Vector2.Lerp(
                            left,
                            top,
                            traveledDistance / leftTopLength);
                    }
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
