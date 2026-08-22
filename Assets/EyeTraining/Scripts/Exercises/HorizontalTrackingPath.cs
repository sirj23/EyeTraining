using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalTrackingPath : ITrackingPath
    {
        private const float TargetViewportY = 0.48f;
        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float leftLimit = TrackingTrainingArea.GetCenterLeft(targetExtentsInViewport);
            float rightLimit = TrackingTrainingArea.GetCenterRight(targetExtentsInViewport);
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float travelDistance = GetOneWayLength(targetExtentsInViewport);
            float viewportX = leftLimit
                + Mathf.PingPong(
                    (float)elapsedTime * TrackingMotionSettings.LinearSpeed,
                    travelDistance) * aspectCorrection;

            return new Vector2(viewportX, TargetViewportY);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            return GetOneWayLength(targetExtentsInViewport) * 2f;
        }

        private static float GetOneWayLength(Vector2 targetExtentsInViewport)
        {
            float aspectCorrection = targetExtentsInViewport.x / targetExtentsInViewport.y;
            float viewportWidth =
                TrackingTrainingArea.GetCenterRight(targetExtentsInViewport)
                - TrackingTrainingArea.GetCenterLeft(targetExtentsInViewport);

            return viewportWidth / aspectCorrection;
        }
    }
}
