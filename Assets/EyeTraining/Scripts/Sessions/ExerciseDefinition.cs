using System;

namespace EyeTraining.Sessions
{
    public sealed class ExerciseDefinition
    {
        public ExerciseDefinition(
            string id,
            string displayName,
            ExerciseFamily family,
            ExercisePriority priority,
            TimeSpan? estimatedDuration,
            bool requiresBreakAfter,
            bool canAppearInMilestoneSession)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Exercise display name cannot be empty.", nameof(displayName));
            }

            if (!Enum.IsDefined(typeof(ExerciseFamily), family))
            {
                throw new ArgumentOutOfRangeException(nameof(family));
            }

            if (!Enum.IsDefined(typeof(ExercisePriority), priority))
            {
                throw new ArgumentOutOfRangeException(nameof(priority));
            }

            if (estimatedDuration.HasValue && estimatedDuration.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(estimatedDuration),
                    "Estimated duration must be positive when provided.");
            }

            Id = id;
            DisplayName = displayName;
            Family = family;
            Priority = priority;
            EstimatedDuration = estimatedDuration;
            RequiresBreakAfter = requiresBreakAfter;
            CanAppearInMilestoneSession = canAppearInMilestoneSession;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public ExerciseFamily Family { get; }

        public ExercisePriority Priority { get; }

        public TimeSpan? EstimatedDuration { get; }

        public bool RequiresBreakAfter { get; }

        public bool CanAppearInMilestoneSession { get; }
    }
}
