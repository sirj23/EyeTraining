using UnityEngine;

namespace EyeTraining.Exercises.Saccades
{
    public readonly struct NumberJourneyLayoutItem
    {
        public NumberJourneyLayoutItem(
            int number,
            Vector2 viewportPosition,
            float rotationDegrees)
        {
            Number = number;
            ViewportPosition = viewportPosition;
            RotationDegrees = rotationDegrees;
        }

        public int Number { get; }

        public Vector2 ViewportPosition { get; }

        public float RotationDegrees { get; }
    }
}
