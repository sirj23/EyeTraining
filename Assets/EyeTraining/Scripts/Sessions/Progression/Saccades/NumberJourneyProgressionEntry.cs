using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Sessions.Progression.Saccades
{
    public sealed class NumberJourneyProgressionEntry
    {
        public NumberJourneyProgressionEntry(
            int completedSessionNumber,
            int appliedLevel,
            ExerciseCompletionStatus completionStatus)
        {
            if (completedSessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionNumber));
            }

            if (appliedLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(appliedLevel));
            }

            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(completionStatus));
            }

            CompletedSessionNumber = completedSessionNumber;
            AppliedLevel = appliedLevel;
            CompletionStatus = completionStatus;
        }

        public int CompletedSessionNumber { get; }

        public int AppliedLevel { get; }

        public ExerciseCompletionStatus CompletionStatus { get; }
    }
}
