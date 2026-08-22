using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalZigzagTrackingPath : ITrackingPath
    {
        private const int SegmentCount = 6;
        private const float HorizontalCoverage = 0.93f;
        private const float HalfWidthInViewportHeight = 0.68f;
        private const float AmplitudeInViewportHeight = 0.11f;

        private readonly Vector2[] points = new Vector2[SegmentCount + 1];

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            BuildPoints(targetExtentsInViewport, aspectCorrection);
            float pathLength = GetOneWayLength();

            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed),
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

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            BuildPoints(targetExtentsInViewport, aspectCorrection);
            return GetOneWayLength() * 2f;
        }

        private float GetOneWayLength()
        {
            float length = 0f;

            for (int index = 0; index < SegmentCount; index++)
            {
                length += Vector2.Distance(points[index], points[index + 1]);
            }

            return length;
        }

        private void BuildPoints(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            float availableHalfWidth =
                (TrackingTrainingArea.Center.x
                    - TrackingTrainingArea.GetCenterLeft(targetExtentsInViewport))
                / aspectCorrection;
            float halfWidth = Mathf.Min(
                HalfWidthInViewportHeight,
                availableHalfWidth * HorizontalCoverage);
            float left = -halfWidth;
            float right = halfWidth;
            float verticalSpace = Mathf.Min(
                TrackingTrainingArea.Center.y - TrackingTrainingArea.GetCenterBottom(targetExtentsInViewport),
                TrackingTrainingArea.GetCenterTop(targetExtentsInViewport) - TrackingTrainingArea.Center.y);
            float amplitude = Mathf.Min(AmplitudeInViewportHeight, verticalSpace);

            for (int index = 0; index <= SegmentCount; index++)
            {
                float progress = (float)index / SegmentCount;
                float y = TrackingTrainingArea.Center.y + (index % 2 == 0 ? amplitude : -amplitude);
                points[index] = new Vector2(Mathf.Lerp(left, right, progress), y);
            }
        }

        private static Vector2 ToViewport(Vector2 localPosition, float aspectCorrection)
        {
            return new Vector2(
                TrackingTrainingArea.Center.x + localPosition.x * aspectCorrection,
                localPosition.y);
        }
    }
}
