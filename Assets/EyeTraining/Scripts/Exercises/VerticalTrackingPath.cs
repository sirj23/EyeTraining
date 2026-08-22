using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class VerticalTrackingPath : ITrackingPath
    {
        private const float TargetViewportX = 0.5f;
        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            float bottomLimit = TrackingTrainingArea.GetCenterBottom(targetExtentsInViewport);
            float travelDistance = GetOneWayLength(targetExtentsInViewport);
            float viewportY = bottomLimit
                + Mathf.PingPong(
                    (float)elapsedTime * TrackingMotionSettings.LinearSpeed,
                    travelDistance);

            return new Vector2(TargetViewportX, viewportY);
        }

        public float GetFullCycleLength(Vector2 targetExtentsInViewport)
        {
            return GetOneWayLength(targetExtentsInViewport) * 2f;
        }

        private static float GetOneWayLength(Vector2 targetExtentsInViewport)
        {
            return TrackingTrainingArea.GetCenterTop(targetExtentsInViewport)
                - TrackingTrainingArea.GetCenterBottom(targetExtentsInViewport);
        }
    }
}
