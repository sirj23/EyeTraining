using System;
using UnityEngine;
using EyeTraining.Sessions.Progression.Peripheral;

namespace EyeTraining.Exercises.Peripheral
{
    public static class PeripheralLayout
    {
        public const float SafeLeft = 0.10f;
        public const float SafeRight = 0.90f;
        public const float SafeBottom = 0.15f;
        public const float SafeTop = 0.85f;

        public static Vector2 GetViewportPosition(PeripheralDirection direction, EdgeSignalsLevelSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            float horizontal = settings.HorizontalOffsetViewport;
            float vertical = settings.VerticalOffsetViewport;
            return direction switch
            {
                PeripheralDirection.Up => new Vector2(0.5f, 0.5f + vertical),
                PeripheralDirection.UpRight => new Vector2(0.5f + horizontal, 0.5f + vertical),
                PeripheralDirection.Right => new Vector2(0.5f + horizontal, 0.5f),
                PeripheralDirection.DownRight => new Vector2(0.5f + horizontal, 0.5f - vertical),
                PeripheralDirection.Down => new Vector2(0.5f, 0.5f - vertical),
                PeripheralDirection.DownLeft => new Vector2(0.5f - horizontal, 0.5f - vertical),
                PeripheralDirection.Left => new Vector2(0.5f - horizontal, 0.5f),
                PeripheralDirection.UpLeft => new Vector2(0.5f - horizontal, 0.5f + vertical),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public static bool IsFullyInsideSafeArea(PeripheralDirection direction, float aspectRatio, EdgeSignalsLevelSettings settings)
        {
            if (aspectRatio <= 0f) throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            Vector2 position = GetViewportPosition(direction, settings);
            float halfHeight = settings.StimulusSizeViewportHeight * 0.5f;
            float halfWidth = halfHeight / aspectRatio;
            return position.x - halfWidth >= SafeLeft && position.x + halfWidth <= SafeRight
                && position.y - halfHeight >= SafeBottom && position.y + halfHeight <= SafeTop;
        }
    }
}
