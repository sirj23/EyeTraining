using System;
using UnityEngine;

namespace EyeTraining.Exercises.Peripheral
{
    public static class PeripheralLayout
    {
        public const float StimulusSizeViewportHeight = 0.055f;
        public const float HorizontalOffset = 0.35f;
        public const float VerticalOffset = 0.285f;
        public const float SafeLeft = 0.10f;
        public const float SafeRight = 0.90f;
        public const float SafeBottom = 0.15f;
        public const float SafeTop = 0.85f;

        public static Vector2 GetViewportPosition(PeripheralDirection direction)
        {
            return direction switch
            {
                PeripheralDirection.Up => new Vector2(0.5f, 0.5f + VerticalOffset),
                PeripheralDirection.UpRight => new Vector2(0.5f + HorizontalOffset, 0.5f + VerticalOffset),
                PeripheralDirection.Right => new Vector2(0.5f + HorizontalOffset, 0.5f),
                PeripheralDirection.DownRight => new Vector2(0.5f + HorizontalOffset, 0.5f - VerticalOffset),
                PeripheralDirection.Down => new Vector2(0.5f, 0.5f - VerticalOffset),
                PeripheralDirection.DownLeft => new Vector2(0.5f - HorizontalOffset, 0.5f - VerticalOffset),
                PeripheralDirection.Left => new Vector2(0.5f - HorizontalOffset, 0.5f),
                PeripheralDirection.UpLeft => new Vector2(0.5f - HorizontalOffset, 0.5f + VerticalOffset),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public static bool IsFullyInsideSafeArea(PeripheralDirection direction, float aspectRatio)
        {
            if (aspectRatio <= 0f) throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            Vector2 position = GetViewportPosition(direction);
            float halfHeight = StimulusSizeViewportHeight * 0.5f;
            float halfWidth = halfHeight / aspectRatio;
            return position.x - halfWidth >= SafeLeft && position.x + halfWidth <= SafeRight
                && position.y - halfHeight >= SafeBottom && position.y + halfHeight <= SafeTop;
        }
    }
}
