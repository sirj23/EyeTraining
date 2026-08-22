using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class SpiralTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float RevolutionCount = 1.5f;
        private const float OuterRadiusInViewportHeight = 0.21f;
        private const float InnerRadiusInViewportHeight = 0.03f;

        private readonly float[] cumulativeArcLengths = new float[ArcLengthSegments + 1];
        private Vector2 cachedTargetExtents = new(float.NaN, float.NaN);
        private float aspectCorrection;
        private float safeScale;
        private float pathLength;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            EnsureArcLengthTable(targetExtentsInViewport);
            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed),
                pathLength);
            float progress = FindProgressForArcLength(traveledDistance);

            return ToViewport(EvaluateLocalPoint(progress));
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            EnsureArcLengthTable(targetExtentsInViewport);
            return pathLength * 2f;
        }

        private void EnsureArcLengthTable(Vector2 targetExtentsInViewport)
        {
            if (targetExtentsInViewport == cachedTargetExtents)
            {
                return;
            }

            cachedTargetExtents = targetExtentsInViewport;
            aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            safeScale = TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                OuterRadiusInViewportHeight,
                OuterRadiusInViewportHeight);
            cumulativeArcLengths[0] = 0f;
            Vector2 previousPoint = EvaluateLocalPoint(0f);

            for (int index = 1; index <= ArcLengthSegments; index++)
            {
                Vector2 point = EvaluateLocalPoint((float)index / ArcLengthSegments);
                cumulativeArcLengths[index] =
                    cumulativeArcLengths[index - 1] + Vector2.Distance(previousPoint, point);
                previousPoint = point;
            }

            pathLength = cumulativeArcLengths[ArcLengthSegments];
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

        private Vector2 EvaluateLocalPoint(float progress)
        {
            float angle = Mathf.PI * 2f * RevolutionCount * progress;
            float radius = Mathf.Lerp(
                OuterRadiusInViewportHeight,
                InnerRadiusInViewportHeight,
                progress) * safeScale;

            return new Vector2(
                radius * Mathf.Cos(angle),
                -radius * Mathf.Sin(angle));
        }

        private Vector2 ToViewport(Vector2 localPosition)
        {
            return new Vector2(
                TrackingTrainingArea.Center.x + localPosition.x * aspectCorrection,
                TrackingTrainingArea.Center.y + localPosition.y);
        }
    }
}
