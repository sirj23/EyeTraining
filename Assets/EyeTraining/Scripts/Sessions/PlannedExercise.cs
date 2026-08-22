using System;

namespace EyeTraining.Sessions
{
    public sealed class PlannedExercise
    {
        public PlannedExercise(
            ExerciseDefinition definition,
            TimeSpan plannedDuration,
            int order,
            SessionExerciseRole role,
            IExerciseParameters parameters = null)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (plannedDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(plannedDuration),
                    "Planned duration must be positive.");
            }

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
            }

            if (!Enum.IsDefined(typeof(SessionExerciseRole), role))
            {
                throw new ArgumentOutOfRangeException(nameof(role));
            }

            Definition = definition;
            PlannedDuration = plannedDuration;
            Order = order;
            Role = role;
            Parameters = parameters;
        }

        public ExerciseDefinition Definition { get; }

        public TimeSpan PlannedDuration { get; }

        public int Order { get; }

        public SessionExerciseRole Role { get; }

        public IExerciseParameters Parameters { get; }
    }
}
