using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class UShapeTrackingPath : ITrackingPath
    {
        private const int ArcLengthSegments = 256;
        private const float HalfWidthInViewportHeight = 0.25f;
        private const float HalfHeightInViewportHeight = 0.185f;
        private const float ArcRadiusYInViewportHeight = 0.16f;
        private const float StraightLengthInViewportHeight = 0.21f;

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
                (float)(elapsedTime * TrackingMotionSettings.LinearSpeed),
                pathLength);
            Vector2 localPosition;
            float halfWidth = HalfWidthInViewportHeight * safeScale;
            float top = HalfHeightInViewportHeight * safeScale;
            float arcCenterY =
                (-HalfHeightInViewportHeight + ArcRadiusYInViewportHeight) * safeScale;

            if (traveledDistance < straightLength)
            {
                localPosition = new Vector2(-halfWidth, top - traveledDistance);
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
                    localPosition = new Vector2(halfWidth, arcCenterY + traveledDistance);
                }
            }

            return ToViewport(localPosition);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            EnsurePathData(targetExtentsInViewport);
            return pathLength * 2f;
        }

        private void EnsurePathData(Vector2 targetExtentsInViewport)
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
                HalfWidthInViewportHeight,
                HalfHeightInViewportHeight);
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
            float angle = Mathf.PI * (1f + progress);
            float arcCenterY =
                (-HalfHeightInViewportHeight + ArcRadiusYInViewportHeight) * safeScale;

            return new Vector2(
                HalfWidthInViewportHeight * safeScale * Mathf.Cos(angle),
                arcCenterY + ArcRadiusYInViewportHeight * safeScale * Mathf.Sin(angle));
        }

        private Vector2 ToViewport(Vector2 localPosition)
        {
            return new Vector2(
                TrackingTrainingArea.Center.x + localPosition.x * aspectCorrection,
                TrackingTrainingArea.Center.y + localPosition.y);
        }
    }
}
