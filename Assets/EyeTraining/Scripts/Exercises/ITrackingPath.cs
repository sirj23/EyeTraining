using UnityEngine;

namespace EyeTraining.Exercises
{
    public interface ITrackingPath
    {
        Vector2 Evaluate(double elapsedTime, Vector2 targetExtentsInViewport);
    }
}
