using System;
using EyeTraining.Sessions.Progression.Tracking;

namespace EyeTraining.Sessions.Scheduling
{
    public interface ITrackingDurationEstimator
    {
        TimeSpan Estimate(string exerciseId, TrackingExerciseParameters parameters);
    }
}
