using System;
using EyeTraining.Exercises;
using EyeTraining.Sessions.Progression.Tracking;
using UnityEngine;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class ReferenceTrackingDurationEstimator : ITrackingDurationEstimator
    {
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;
        public const float ReferenceTargetDiameterInPixels = 76f;

        private static readonly Vector2 ReferenceTargetExtentsInViewport = new Vector2(
            ReferenceTargetDiameterInPixels * 0.5f / ReferenceWidth,
            ReferenceTargetDiameterInPixels * 0.5f / ReferenceHeight);

        private readonly TrackingExerciseCatalog _catalog;

        public ReferenceTrackingDurationEstimator(TrackingExerciseCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public TimeSpan Estimate(string exerciseId, TrackingExerciseParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            TrackingExerciseCatalogEntry entry = _catalog.Get(exerciseId);
            ITrackingPath path = entry.CreatePath();
            float fullCycleLength = path.GetFullCycleLength(ReferenceTargetExtentsInViewport);
            double durationSeconds = fullCycleLength
                / (TrackingMotionSettings.LinearSpeed * parameters.SpeedMultiplier)
                * parameters.CycleCount;

            return TimeSpan.FromSeconds(durationSeconds);
        }
    }
}
