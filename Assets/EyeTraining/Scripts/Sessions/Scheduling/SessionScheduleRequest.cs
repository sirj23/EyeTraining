using System;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Rotation;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class SessionScheduleRequest
    {
        public SessionScheduleRequest(
            int currentSessionNumber,
            int completedSessionCount,
            RotationHistory rotationHistory,
            TrackingProgressionHistory trackingProgressionHistory)
        {
            if (currentSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            }

            if (completedSessionCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionCount));
            }

            if (currentSessionNumber != completedSessionCount + 1)
            {
                throw new ArgumentException(
                    "Current session number must be exactly one greater than completed session count.");
            }

            CurrentSessionNumber = currentSessionNumber;
            CompletedSessionCount = completedSessionCount;
            RotationHistory = rotationHistory ?? throw new ArgumentNullException(nameof(rotationHistory));
            TrackingProgressionHistory = trackingProgressionHistory
                ?? throw new ArgumentNullException(nameof(trackingProgressionHistory));
        }

        public int CurrentSessionNumber { get; }

        public int CompletedSessionCount { get; }

        public RotationHistory RotationHistory { get; }

        public TrackingProgressionHistory TrackingProgressionHistory { get; }
    }
}
