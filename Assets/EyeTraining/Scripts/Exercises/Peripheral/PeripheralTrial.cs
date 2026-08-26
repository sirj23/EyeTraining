using System;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class PeripheralTrial
    {
        public PeripheralTrial(PeripheralDirection direction, float delayBeforeStimulus)
        {
            if (!Enum.IsDefined(typeof(PeripheralDirection), direction))
                throw new ArgumentOutOfRangeException(nameof(direction));
            if (delayBeforeStimulus < PeripheralStimulusSequence.MinimumDelay
                || delayBeforeStimulus > PeripheralStimulusSequence.MaximumDelay)
                throw new ArgumentOutOfRangeException(nameof(delayBeforeStimulus));

            Direction = direction;
            DelayBeforeStimulus = delayBeforeStimulus;
        }

        public PeripheralDirection Direction { get; }
        public float DelayBeforeStimulus { get; }
    }
}
