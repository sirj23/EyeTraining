using System;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class EdgeSignalsRound
    {
        private readonly int trialCount;
        private readonly double responseWindowSeconds;

        private bool responseWindowOpen;
        private bool currentTrialDetected;
        private double stimulusAppearedAt;
        private double totalReactionTime;

        public int CompletedTrialCount { get; private set; }
        public int DetectedCount { get; private set; }
        public int MissedCount { get; private set; }
        public bool IsInterrupted { get; private set; }
        public EdgeSignalsRound(int trialCount, double responseWindowSeconds)
        {
            if (trialCount <= 0 || responseWindowSeconds <= 0d) throw new ArgumentOutOfRangeException();
            this.trialCount = trialCount; this.responseWindowSeconds = responseWindowSeconds;
        }
        public bool IsComplete => CompletedTrialCount == trialCount;
        public bool IsResponseWindowOpen => responseWindowOpen;

        public double? MeanReactionTimeSeconds => DetectedCount == 0
            ? null
            : totalReactionTime / DetectedCount;

        public void BeginTrial(double appearanceTime)
        {
            if (IsComplete || IsInterrupted || responseWindowOpen)
                throw new InvalidOperationException("A new trial cannot begin now.");
            if (double.IsNaN(appearanceTime) || double.IsInfinity(appearanceTime) || appearanceTime < 0d)
                throw new ArgumentOutOfRangeException(nameof(appearanceTime));
            stimulusAppearedAt = appearanceTime;
            currentTrialDetected = false;
            responseWindowOpen = true;
        }

        public bool TryRespond(double responseTime)
        {
            if (!responseWindowOpen || currentTrialDetected) return false;
            double reactionTime = responseTime - stimulusAppearedAt;
            if (reactionTime < 0d || reactionTime > responseWindowSeconds) return false;
            currentTrialDetected = true;
            DetectedCount++;
            totalReactionTime += reactionTime;
            return true;
        }

        public void CloseTrial(double closeTime)
        {
            if (!responseWindowOpen) throw new InvalidOperationException("No trial is active.");
            if (closeTime - stimulusAppearedAt + 0.000000001d < responseWindowSeconds)
                throw new ArgumentOutOfRangeException(nameof(closeTime));
            if (!currentTrialDetected) MissedCount++;
            CompletedTrialCount++;
            responseWindowOpen = false;
        }

        public void Interrupt()
        {
            responseWindowOpen = false;
            IsInterrupted = true;
        }
    }
}
