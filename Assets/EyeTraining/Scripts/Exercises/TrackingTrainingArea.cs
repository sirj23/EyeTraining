using UnityEngine;

namespace EyeTraining.Exercises
{
    public static class TrackingTrainingArea
    {
        public const float Left = 0.07f;
        public const float Right = 0.93f;
        public const float Bottom = 0.15f;
        public const float Top = 0.81f;

        public const float Width = Right - Left;
        public const float Height = Top - Bottom;

        public static readonly Vector2 Center = new(
            (Left + Right) * 0.5f,
            (Bottom + Top) * 0.5f);

        public static float GetCenterLeft(Vector2 targetExtentsInViewport)
        {
            return Left + targetExtentsInViewport.x;
        }

        public static float GetCenterRight(Vector2 targetExtentsInViewport)
        {
            return Right - targetExtentsInViewport.x;
        }

        public static float GetCenterBottom(Vector2 targetExtentsInViewport)
        {
            return Bottom + targetExtentsInViewport.y;
        }

        public static float GetCenterTop(Vector2 targetExtentsInViewport)
        {
            return Top - targetExtentsInViewport.y;
        }

        public static float GetCenteredShapeScale(
            Vector2 targetExtentsInViewport,
            float aspectCorrection,
            float halfWidthInViewportHeight,
            float halfHeightInViewportHeight)
        {
            float horizontalSpace = Mathf.Min(
                Center.x - GetCenterLeft(targetExtentsInViewport),
                GetCenterRight(targetExtentsInViewport) - Center.x) / aspectCorrection;
            float verticalSpace = Mathf.Min(
                Center.y - GetCenterBottom(targetExtentsInViewport),
                GetCenterTop(targetExtentsInViewport) - Center.y);
            float horizontalScale = halfWidthInViewportHeight > 0f
                ? horizontalSpace / halfWidthInViewportHeight
                : float.PositiveInfinity;
            float verticalScale = halfHeightInViewportHeight > 0f
                ? verticalSpace / halfHeightInViewportHeight
                : float.PositiveInfinity;

            return Mathf.Clamp01(Mathf.Min(horizontalScale, verticalScale));
        }
    }
}
