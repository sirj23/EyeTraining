using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalEllipseTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float HorizontalCoverage = 0.88f;
        private const float RadiusXInViewportHeight = 0.62f;
        private const float RadiusYInViewportHeight = 0.19f;

        private readonly float[] cumulativeArcLengths;
        private readonly float totalArcLength;

        public HorizontalEllipseTrackingPath()
        {
            cumulativeArcLengths = new float[ArcLengthSegments + 1];
            Vector2 previousPoint = EvaluateLocalPoint(0f);

            for (int index = 1; index <= ArcLengthSegments; index++)
            {
                float angle = index * Mathf.PI * 2f / ArcLengthSegments;
                Vector2 point = EvaluateLocalPoint(angle);
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
            float fullCycleLength = totalArcLength * safeScale;
            float traveledDistance =
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % fullCycleLength);
            float targetArcLength = traveledDistance / safeScale;
            float angle = FindAngleForArcLength(targetArcLength);
            float viewportX = TrackingTrainingArea.Center.x + radiusX * Mathf.Sin(angle);
            float viewportY = TrackingTrainingArea.Center.y
                + RadiusYInViewportHeight * safeScale * Mathf.Cos(angle);

            return new Vector2(viewportX, viewportY);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            return totalArcLength * GetSafeScale(targetExtentsInViewport, aspectCorrection);
        }

        private float FindAngleForArcLength(float targetArcLength)
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
            float samplePosition = lowerIndex + segmentProgress;

            return samplePosition * Mathf.PI * 2f / ArcLengthSegments;
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

        private static Vector2 EvaluateLocalPoint(float angle)
        {
            return new Vector2(
                RadiusXInViewportHeight * Mathf.Sin(angle),
                RadiusYInViewportHeight * Mathf.Cos(angle));
        }
    }
}
