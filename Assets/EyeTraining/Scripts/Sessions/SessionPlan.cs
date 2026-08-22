using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions
{
    public sealed class SessionPlan
    {
        public SessionPlan(SessionType sessionType, IEnumerable<PlannedExercise> exercises)
        {
            if (!Enum.IsDefined(typeof(SessionType), sessionType))
            {
                throw new ArgumentOutOfRangeException(nameof(sessionType));
            }

            if (exercises == null)
            {
                throw new ArgumentNullException(nameof(exercises));
            }

            var orderedExercises = new List<PlannedExercise>(exercises);
            ValidateExercises(orderedExercises);
            orderedExercises.Sort((left, right) => left.Order.CompareTo(right.Order));

            SessionType = sessionType;
            Exercises = orderedExercises.AsReadOnly();
            PreparationExercises = SelectByRole(orderedExercises, SessionExerciseRole.Preparation);
            MainExercises = SelectByRole(orderedExercises, SessionExerciseRole.Main);
            FinalExercises = SelectByRole(orderedExercises, SessionExerciseRole.Final);
            EstimatedDuration = SumDuration(orderedExercises);
        }

        public SessionType SessionType { get; }

        public IReadOnlyList<PlannedExercise> Exercises { get; }

        /// <summary>
        /// Sum of all planned exercise durations. Explicit breaks will be included
        /// when they become elements of the session plan model.
        /// </summary>
        public TimeSpan EstimatedDuration { get; }

        public IReadOnlyList<PlannedExercise> PreparationExercises { get; }

        public IReadOnlyList<PlannedExercise> MainExercises { get; }

        public IReadOnlyList<PlannedExercise> FinalExercises { get; }

        private static void ValidateExercises(IReadOnlyList<PlannedExercise> exercises)
        {
            var orders = new HashSet<int>();

            for (var index = 0; index < exercises.Count; index++)
            {
                PlannedExercise exercise = exercises[index];
                if (exercise == null)
                {
                    throw new ArgumentException("Session plan cannot contain null exercises.", nameof(exercises));
                }

                if (!orders.Add(exercise.Order))
                {
                    throw new ArgumentException("Exercise order values must be unique.", nameof(exercises));
                }
            }
        }

        private static IReadOnlyList<PlannedExercise> SelectByRole(
            IReadOnlyList<PlannedExercise> exercises,
            SessionExerciseRole role)
        {
            var selected = new List<PlannedExercise>();

            for (var index = 0; index < exercises.Count; index++)
            {
                PlannedExercise exercise = exercises[index];
                if (exercise.Role == role)
                {
                    selected.Add(exercise);
                }
            }

            return selected.AsReadOnly();
        }

        private static TimeSpan SumDuration(IReadOnlyList<PlannedExercise> exercises)
        {
            var duration = TimeSpan.Zero;

            for (var index = 0; index < exercises.Count; index++)
            {
                duration += exercises[index].PlannedDuration;
            }

            return duration;
        }
    }
}
