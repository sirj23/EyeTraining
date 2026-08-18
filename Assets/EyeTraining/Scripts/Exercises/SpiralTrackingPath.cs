using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class SpiralTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float RevolutionCount = 1.5f;
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float OuterRadiusInViewportHeight = 0.21f;
        private const float InnerRadiusInViewportHeight = 0.03f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        private readonly float[] cumulativeArcLengths = new float[ArcLengthSegments + 1];
        private Vector2 cachedTargetExtents = new(float.NaN, float.NaN);
        private float aspectCorrection;
        private float safeScale;
        private float pathLength;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            EnsureArcLengthTable(targetExtentsInViewport);
            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * LinearSpeed),
                pathLength);
            float progress = FindProgressForArcLength(traveledDistance);

            return ToViewport(EvaluateLocalPoint(progress));
        }

        private void EnsureArcLengthTable(Vector2 targetExtentsInViewport)
        {
            if (targetExtentsInViewport == cachedTargetExtents)
            {
                return;
            }

            cachedTargetExtents = targetExtentsInViewport;
            aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float horizontalSpace =
                0.5f - HorizontalViewportMargin - targetExtentsInViewport.x;
            float bottomSpace =
                CenterViewportY - BottomViewportLimit - targetExtentsInViewport.y;
            float topSpace =
                TopViewportLimit - CenterViewportY - targetExtentsInViewport.y;
            float horizontalScale =
                horizontalSpace / (OuterRadiusInViewportHeight * aspectCorrection);
            float verticalScale =
                Mathf.Min(bottomSpace, topSpace) / OuterRadiusInViewportHeight;
            safeScale = Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
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
                CenterViewportX + localPosition.x * aspectCorrection,
                CenterViewportY + localPosition.y);
        }
    }
}
