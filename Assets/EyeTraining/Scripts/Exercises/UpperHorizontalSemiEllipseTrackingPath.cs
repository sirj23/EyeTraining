using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class UpperHorizontalSemiEllipseTrackingPath : ITrackingPath
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
        private readonly double traverseDurationSeconds;

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
            float linearSpeed =
                Mathf.PI * 2f * CircleRadiusInViewportHeight /
                CircleRevolutionDurationSeconds;
            traverseDurationSeconds = totalArcLength / linearSpeed;
        }

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float radiusX = RadiusXInViewportHeight * aspectCorrection;
            float pingPongProgress = Mathf.PingPong(
                (float)(elapsedTime / traverseDurationSeconds),
                1f);
            float arcProgress = FindProgressForArcLength(pingPongProgress * totalArcLength);
            float angle = Mathf.PI * (1f - arcProgress);
            float viewportX = CenterViewportX + radiusX * Mathf.Cos(angle);
            float viewportY = CenterViewportY + RadiusYInViewportHeight * Mathf.Sin(angle);

            return new Vector2(viewportX, viewportY);
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

        private static Vector2 EvaluateLocalPoint(float progress)
        {
            float angle = Mathf.PI * (1f - progress);

            return new Vector2(
                RadiusXInViewportHeight * Mathf.Cos(angle),
                RadiusYInViewportHeight * Mathf.Sin(angle));
        }
    }
}
