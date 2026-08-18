using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalWaveTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float WavePeriods = 2f;
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.32f;
        private const float AmplitudeInViewportHeight = 0.10f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        private readonly float[] cumulativeArcLengths = new float[ArcLengthSegments + 1];
        private Vector2 cachedTargetExtents = new(float.NaN, float.NaN);
        private float aspectCorrection;
        private float left;
        private float right;
        private float amplitude;
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
            float horizontalScale = Mathf.Clamp01(
                horizontalSpace / (HalfWidthInViewportHeight * aspectCorrection));
            float halfWidth = HalfWidthInViewportHeight * horizontalScale;
            left = -halfWidth;
            right = halfWidth;
            float verticalSpace = Mathf.Min(
                CenterViewportY - BottomViewportLimit - targetExtentsInViewport.y,
                TopViewportLimit - CenterViewportY - targetExtentsInViewport.y);
            amplitude = Mathf.Min(AmplitudeInViewportHeight, verticalSpace);
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
            return new Vector2(
                Mathf.Lerp(left, right, progress),
                CenterViewportY + amplitude * Mathf.Sin(Mathf.PI * 2f * WavePeriods * progress));
        }

        private Vector2 ToViewport(Vector2 localPosition)
        {
            return new Vector2(
                CenterViewportX + localPosition.x * aspectCorrection,
                localPosition.y);
        }
    }
}
