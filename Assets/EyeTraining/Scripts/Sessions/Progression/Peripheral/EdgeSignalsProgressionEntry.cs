using System;
using EyeTraining.Sessions.History;

namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsProgressionEntry
    {
        public EdgeSignalsProgressionEntry(int completedSessionNumber, int appliedLevel, ExerciseCompletionStatus status)
        {
            if (completedSessionNumber <= 0 || appliedLevel < 0) throw new ArgumentOutOfRangeException(nameof(completedSessionNumber));
            if (!Enum.IsDefined(typeof(ExerciseCompletionStatus), status)) throw new ArgumentOutOfRangeException(nameof(status));
            CompletedSessionNumber = completedSessionNumber; AppliedLevel = appliedLevel; CompletionStatus = status;
        }
        public int CompletedSessionNumber { get; }
        public int AppliedLevel { get; }
        public ExerciseCompletionStatus CompletionStatus { get; }
    }
}
