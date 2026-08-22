using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class SquareTrackingPath : IClosedTrackingPath
    {
        private const float HalfSideInViewportHeight = 0.18f;

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float halfSide = GetSafeScale(targetExtentsInViewport, aspectCorrection)
                * HalfSideInViewportHeight;
            float sideLength = halfSide * 2f;
            float perimeter = sideLength * 4f;
            float traveledDistance = (float)(elapsedTime * TrackingMotionSettings.LinearSpeed % perimeter);
            float halfViewportWidth = halfSide * aspectCorrection;
            float left = TrackingTrainingArea.Center.x - halfViewportWidth;
            float right = TrackingTrainingArea.Center.x + halfViewportWidth;
            float top = TrackingTrainingArea.Center.y + halfSide;
            float bottom = TrackingTrainingArea.Center.y - halfSide;

            if (traveledDistance < sideLength)
            {
                return new Vector2(
                    left + traveledDistance * aspectCorrection,
                    top);
            }

            traveledDistance -= sideLength;

            if (traveledDistance < sideLength)
            {
                return new Vector2(right, top - traveledDistance);
            }

            traveledDistance -= sideLength;

            if (traveledDistance < sideLength)
            {
                return new Vector2(
                    right - traveledDistance * aspectCorrection,
                    bottom);
            }

            traveledDistance -= sideLength;
            return new Vector2(left, bottom + traveledDistance);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float halfSide = GetSafeScale(targetExtentsInViewport, aspectCorrection)
                * HalfSideInViewportHeight;

            return halfSide * 8f;
        }

        private static float GetSafeScale(
            Vector2 targetExtentsInViewport,
            float aspectCorrection)
        {
            return TrackingTrainingArea.GetCenteredShapeScale(
                targetExtentsInViewport,
                aspectCorrection,
                HalfSideInViewportHeight,
                HalfSideInViewportHeight);
        }
    }
}
