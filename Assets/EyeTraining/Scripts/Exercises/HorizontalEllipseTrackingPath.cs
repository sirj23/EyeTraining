using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalEllipseTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float RadiusXInViewportHeight = 0.32f;
        private const float RadiusYInViewportHeight = 0.16f;
        private const float CircleRadiusInViewportHeight = 0.18f;
        private const float CircleRevolutionDurationSeconds = 10f;

        private readonly float[] cumulativeArcLengths;
        private readonly float totalArcLength;
        private readonly double revolutionDurationSeconds;

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
            float circleLinearSpeed =
                Mathf.PI * 2f * CircleRadiusInViewportHeight /
                CircleRevolutionDurationSeconds;
            revolutionDurationSeconds = totalArcLength / circleLinearSpeed;
        }

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float radiusX = RadiusXInViewportHeight * aspectCorrection;
            double elapsedInRevolution = elapsedTime % revolutionDurationSeconds;
            float targetArcLength =
                (float)(elapsedInRevolution / revolutionDurationSeconds) * totalArcLength;
            float angle = FindAngleForArcLength(targetArcLength);
            float viewportX = CenterViewportX + radiusX * Mathf.Sin(angle);
            float viewportY = CenterViewportY + RadiusYInViewportHeight * Mathf.Cos(angle);

            return new Vector2(viewportX, viewportY);
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

        private static Vector2 EvaluateLocalPoint(float angle)
        {
            return new Vector2(
                RadiusXInViewportHeight * Mathf.Sin(angle),
                RadiusYInViewportHeight * Mathf.Cos(angle));
        }
    }
}
