using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class InvertedUShapeTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float CenterViewportX = 0.5f;
        private const float CenterViewportY = 0.48f;
        private const float HorizontalViewportMargin = 0.12f;
        private const float BottomViewportLimit = 0.22f;
        private const float TopViewportLimit = 0.74f;
        private const float HalfWidthInViewportHeight = 0.25f;
        private const float HalfHeightInViewportHeight = 0.185f;
        private const float ArcRadiusYInViewportHeight = 0.16f;
        private const float StraightLengthInViewportHeight = 0.21f;
        private const float LinearSpeed = Mathf.PI * 2f * 0.18f / 10f;

        private readonly float[] cumulativeArcLengths = new float[ArcLengthSegments + 1];
        private Vector2 cachedTargetExtents = new(float.NaN, float.NaN);
        private float aspectCorrection;
        private float safeScale;
        private float straightLength;
        private float arcLength;
        private float pathLength;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            EnsurePathData(targetExtentsInViewport);
            float traveledDistance = Mathf.PingPong(
                (float)(elapsedTime * LinearSpeed),
                pathLength);
            Vector2 localPosition;
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float bottom = -HalfHeightInViewportHeight * safeScale;
            float arcCenterY =
                (HalfHeightInViewportHeight - ArcRadiusYInViewportHeight) * safeScale;

            if (traveledDistance < straightLength)
            {
                localPosition = new Vector2(-halfWidth, bottom + traveledDistance);
            }
            else
            {
                traveledDistance -= straightLength;

                if (traveledDistance < arcLength)
                {
                    float progress = FindArcProgress(traveledDistance);
                    localPosition = EvaluateArcPoint(progress);
                }
                else
                {
                    traveledDistance -= arcLength;
                    localPosition = new Vector2(halfWidth, arcCenterY - traveledDistance);
                }
            }

            return ToViewport(localPosition);
        }

        private void EnsurePathData(Vector2 targetExtentsInViewport)
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
                horizontalSpace / (HalfWidthInViewportHeight * aspectCorrection);
            float verticalScale =
                Mathf.Min(bottomSpace, topSpace) / HalfHeightInViewportHeight;
            safeScale = Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
            straightLength = StraightLengthInViewportHeight * safeScale;
            cumulativeArcLengths[0] = 0f;
            Vector2 previousPoint = EvaluateArcPoint(0f);

            for (int index = 1; index <= ArcLengthSegments; index++)
            {
                Vector2 point = EvaluateArcPoint((float)index / ArcLengthSegments);
                cumulativeArcLengths[index] =
                    cumulativeArcLengths[index - 1] + Vector2.Distance(previousPoint, point);
                previousPoint = point;
            }

            arcLength = cumulativeArcLengths[ArcLengthSegments];
            pathLength = straightLength * 2f + arcLength;
        }

        private float FindArcProgress(float targetArcLength)
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

        private Vector2 EvaluateArcPoint(float progress)
        {
            float angle = Mathf.PI * (1f - progress);
            float arcCenterY =
                (HalfHeightInViewportHeight - ArcRadiusYInViewportHeight) * safeScale;

            return new Vector2(
                HalfWidthInViewportHeight * safeScale * Mathf.Cos(angle),
                arcCenterY + ArcRadiusYInViewportHeight * safeScale * Mathf.Sin(angle));
        }

        private Vector2 ToViewport(Vector2 localPosition)
        {
            return new Vector2(
                CenterViewportX + localPosition.x * aspectCorrection,
                CenterViewportY + localPosition.y);
        }
    }
}
