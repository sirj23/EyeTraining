using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalZigzagTrackingPath : ITrackingPath
    {
        private const int SegmentCount = 6;
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.32f;
        private const float AmplitudeInViewportHeight = 0.11f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        private readonly Vector2[] points = new Vector2[SegmentCount + 1];

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            BuildPoints(targetExtentsInViewport, aspectCorrection);
            float pathLength = 0f;

            for (int index = 0; index < SegmentCount; index++)
            {
                pathLength += Vector2.Distance(points[index], points[index + 1]);
            }

            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * LinearSpeed),
                pathLength);

            for (int index = 0; index < SegmentCount; index++)
            {
                float segmentLength = Vector2.Distance(points[index], points[index + 1]);

                if (traveledDistance <= segmentLength)
                {
                    Vector2 localPosition = Vector2.Lerp(
                        points[index],
                        points[index + 1],
                        traveledDistance / segmentLength);
                    return ToViewport(localPosition, aspectCorrection);
                }

                traveledDistance -= segmentLength;
            }

            return ToViewport(points[SegmentCount], aspectCorrection);
        }

        private void BuildPoints(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            float horizontalSpace =
                0.5f - HorizontalViewportMargin - targetExtentsInViewport.x;
            float horizontalScale = Mathf.Clamp01(
                horizontalSpace / (HalfWidthInViewportHeight * aspectCorrection));
            float halfWidth = HalfWidthInViewportHeight * horizontalScale;
            float left = -halfWidth;
            float right = halfWidth;
            float verticalSpace = Mathf.Min(
                CenterViewportY - BottomViewportLimit - targetExtentsInViewport.y,
                TopViewportLimit - CenterViewportY - targetExtentsInViewport.y);
            float amplitude = Mathf.Min(AmplitudeInViewportHeight, verticalSpace);

            for (int index = 0; index <= SegmentCount; index++)
            {
                float progress = (float)index / SegmentCount;
                float y = CenterViewportY + (index % 2 == 0 ? amplitude : -amplitude);
                points[index] = new Vector2(Mathf.Lerp(left, right, progress), y);
            }
        }

        private static Vector2 ToViewport(Vector2 localPosition, float aspectCorrection)
        {
            return new Vector2(
                CenterViewportX + localPosition.x * aspectCorrection,
                localPosition.y);
        }
    }
}
