using UnityEngine;

namespace EyeTraining.Exercises
{
    public sealed class DiagonalUpTrackingPath : ITrackingPath
    {

        public Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport)
        {
            Vector2 start = new Vector2(
                TrackingTrainingArea.GetCenterLeft(targetExtentsInViewport),
                TrackingTrainingArea.GetCenterBottom(targetExtentsInViewport));
            Vector2 end = new Vector2(
                TrackingTrainingArea.GetCenterRight(targetExtentsInViewport),
                TrackingTrainingArea.GetCenterTop(targetExtentsInViewport));
            float pathLength = GetOneWayLength(targetExtentsInViewport);
            float traveledDistance = Mathf.PingPong(
                (float)elapsedTime * TrackingMotionSettings.LinearSpeed,
                pathLength);
            float progress = traveledDistance / pathLength;

            return Vector2.Lerp(start, end, progress);
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
            float viewportHeight =
                TrackingTrainingArea.GetCenterTop(targetExtentsInViewport)
                - TrackingTrainingArea.GetCenterBottom(targetExtentsInViewport);

            return new Vector2(viewportWidth / aspectCorrection, viewportHeight).magnitude;
        }
    }
}
