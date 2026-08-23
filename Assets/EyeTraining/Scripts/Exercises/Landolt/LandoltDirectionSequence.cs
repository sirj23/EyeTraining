using System;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltDirectionSequence
    {
        private static readonly LandoltDirection[] Pattern =
        {
            LandoltDirection.Up, LandoltDirection.Right,
            LandoltDirection.Down, LandoltDirection.Left,
            LandoltDirection.Right, LandoltDirection.Up,
            LandoltDirection.Left, LandoltDirection.Down,
            LandoltDirection.Up, LandoltDirection.Left,
            LandoltDirection.Right, LandoltDirection.Down,
            LandoltDirection.Left, LandoltDirection.Up,
            LandoltDirection.Down, LandoltDirection.Right
        };

        private readonly int offset;

        public LandoltDirectionSequence(int deterministicSeed)
        {
            offset = PositiveModulo(deterministicSeed * 5, Pattern.Length);
        }

        public LandoltDirection GetDirection(int exposureIndex)
        {
            if (exposureIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exposureIndex));
            }

            return Pattern[(offset + exposureIndex) % Pattern.Length];
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
