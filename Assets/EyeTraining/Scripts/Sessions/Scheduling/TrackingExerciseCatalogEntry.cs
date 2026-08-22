using System;
using EyeTraining.Exercises;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class TrackingExerciseCatalogEntry
    {
        private readonly Func<ITrackingPath> _pathFactory;

        public TrackingExerciseCatalogEntry(
            ExerciseDefinition definition,
            Func<ITrackingPath> pathFactory)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _pathFactory = pathFactory ?? throw new ArgumentNullException(nameof(pathFactory));

            if (definition.Family != ExerciseFamily.Tracking)
            {
                throw new ArgumentException(
                    "Tracking catalog entries must belong to the Tracking family.",
                    nameof(definition));
            }
        }

        public ExerciseDefinition Definition { get; }

        public ITrackingPath CreatePath()
        {
            return _pathFactory();
        }
    }
}
