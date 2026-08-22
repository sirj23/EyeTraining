using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class TriangleTrackingPath : ITrackingPath
    {
        private const float HalfWidthInViewportHeight = 0.28f;
        private const float HalfHeightInViewportHeight = 0.18f;

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
            float traveledDistance = (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % perimeter);
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

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float height = HalfHeightInViewportHeight * safeScale * 2f;

            return 2f * Mathf.Sqrt(halfWidth * halfWidth + height * height)
                + halfWidth * 2f;
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
