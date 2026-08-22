using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class UpperHorizontalSemiEllipseTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float HorizontalCoverage = 0.88f;
        private const float RadiusXInViewportHeight = 0.62f;
        private const float RadiusYInViewportHeight = 0.19f;

        private readonly float[] cumulativeArcLengths;
        private readonly float totalArcLength;

        public UpperHorizontalSemiEllipseTrackingPath()
        {
            cumulativeArcLengths = new float[ArcLengthSegments + 1];
            Vector2 previousPoint = EvaluateLocalPoint(0f);

            for (int index = 1; index <= ArcLengthSegments; index++)
            {
                float progress = (float)index / ArcLengthSegments;
                Vector2 point = EvaluateLocalPoint(progress);
                cumulativeArcLengths[index] =
                    cumulativeArcLengths[index - 1] + Vector2.Distance(previousPoint, point);
                previousPoint = point;
            }

            totalArcLength = cumulativeArcLengths[ArcLengthSegments];
        }

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float safeScale = GetSafeScale(targetExtentsInViewport, aspectCorrection);
            float radiusX = RadiusXInViewportHeight * safeScale * aspectCorrection;
            float oneWayLength = totalArcLength * safeScale;
            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed),
                oneWayLength);
            float arcProgress = FindProgressForArcLength(traveledDistance / safeScale);
            float angle = Mathf.PI * (1f - arcProgress);
            float viewportX = TrackingTrainingArea.Center.x + radiusX * Mathf.Cos(angle);
            float viewportY = TrackingTrainingArea.Center.y
                + RadiusYInViewportHeight * safeScale * Mathf.Sin(angle);

            return new Vector2(viewportX, viewportY);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            return totalArcLength * GetSafeScale(targetExtentsInViewport, aspectCorrection) * 2f;
        }

        private float FindProgressForArcLength(float targetArcLength)
        {
            int lowerIndex = 0;
            int upperIndex = ArcLengthSegments;

            while (lowerIndex + 1 < upperIndex)
            {
                int middleIndex = (lowerIndex + upperIndex) / 2;

                if (cumulativeArcLengths[middleIndex] <= targetArcLength)
                {
                    lowerIndex = middleIndex;
                }
                else
                {
                    upperIndex = middleIndex;
                }
            }

            float segmentStartLength = cumulativeArcLengths[lowerIndex];
            float segmentLength = cumulativeArcLengths[upperIndex] - segmentStartLength;
            float segmentProgress = (targetArcLength - segmentStartLength) / segmentLength;

            return (lowerIndex + segmentProgress) / ArcLengthSegments;
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
                availableHalfWidth * HorizontalCoverage / RadiusXInViewportHeight;
            float verticalScale = TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                0f,
                RadiusYInViewportHeight);

            return Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
        }

        private static Vector2 EvaluateLocalPoint(float progress)
        {
            float angle = Mathf.PI * (1f - progress);

            return new Vector2(
                RadiusXInViewportHeight * Mathf.Cos(angle),
                RadiusYInViewportHeight * Mathf.Sin(angle));
        }
    }
}
