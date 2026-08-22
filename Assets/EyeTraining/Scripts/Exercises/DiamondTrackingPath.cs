using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class DiamondTrackingPath : IClosedTrackingPath
    {
        private const float HalfWidthInViewportHeight = 0.24f;
        private const float HalfHeightInViewportHeight = 0.18f;

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
            float traveledDistance = (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % perimeter);
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

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float halfHeight = HalfHeightInViewportHeight * safeScale;

            return 4f * Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
        }

        private static float GetSafeScale(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            return TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                HalfWidthInViewportHeight,
                HalfHeightInViewportHeight);
        }

        private static Vector2 ToViewport(Vector2 localPosition, float aspectCorrection)
        {
            return new Vector2(
                TrackingTrainingArea.Center.x + localPosition.x * aspectCorrection,
                TrackingTrainingArea.Center.y + localPosition.y);
        }
    }
}
