using System;
using EyeTraining.Sessions.Progression.Tracking;
using EyeTraining.Sessions.Progression.Saccades;
using EyeTraining.Sessions.Progression.VisualSearch;
using EyeTraining.Sessions.Progression.Peripheral;
using EyeTraining.Sessions.Rotation;
using EyeTraining.Sessions.Rotation.Returning;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class SessionScheduleRequest
    {
        public SessionScheduleRequest(
            int currentSessionNumber,
            int completedSessionCount,
            RotationHistory rotationHistory,
            TrackingProgressionHistory trackingProgressionHistory,
            NumberJourneyProgressionHistory numberJourneyProgressionHistory,
            ShapeSearchProgressionHistory shapeSearchProgressionHistory,
            EdgeSignalsProgressionHistory edgeSignalsProgressionHistory,
            ReturningExerciseHistory returningExerciseHistory)
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
            NumberJourneyProgressionHistory = numberJourneyProgressionHistory
                ?? throw new ArgumentNullException(nameof(numberJourneyProgressionHistory));
            ShapeSearchProgressionHistory = shapeSearchProgressionHistory
                ?? throw new ArgumentNullException(nameof(shapeSearchProgressionHistory));
            EdgeSignalsProgressionHistory = edgeSignalsProgressionHistory
                ?? throw new ArgumentNullException(nameof(edgeSignalsProgressionHistory));
            ReturningExerciseHistory = returningExerciseHistory
                ?? throw new ArgumentNullException(nameof(returningExerciseHistory));
        }

        public int CurrentSessionNumber { get; }

        public int CompletedSessionCount { get; }

        public RotationHistory RotationHistory { get; }

        public TrackingProgressionHistory TrackingProgressionHistory { get; }

        public NumberJourneyProgressionHistory NumberJourneyProgressionHistory { get; }
        public ShapeSearchProgressionHistory ShapeSearchProgressionHistory { get; }
        public EdgeSignalsProgressionHistory EdgeSignalsProgressionHistory { get; }
        public ReturningExerciseHistory ReturningExerciseHistory { get; }
    }
}
