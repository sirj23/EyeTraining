using System;
using EyeTraining.Sessions.Rotation.Returning;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class DiversitySlotCadencePolicy
    {
        public int GetMinimumGap(int knownNonTrackingExerciseCount)
        {
            if (knownNonTrackingExerciseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(knownNonTrackingExerciseCount));
            return knownNonTrackingExerciseCount < 5 ? 2 : 3;
        }

        public bool CanUseSlot(int currentSessionNumber, int knownNonTrackingExerciseCount,
            ReturningExerciseHistory history)
        {
            if (currentSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            if (history == null) throw new ArgumentNullException(nameof(history));
            int lastUsedSession = 0;
            foreach (ReturningExerciseUsage usage in history.Usages)
                if (usage.LastCompletedSessionNumber > lastUsedSession)
                    lastUsedSession = usage.LastCompletedSessionNumber;
            return lastUsedSession == 0
                || currentSessionNumber - lastUsedSession >= GetMinimumGap(knownNonTrackingExerciseCount);
        }

        public bool ReturningWouldPreserveNextUnlock(int currentSessionNumber,
            int knownNonTrackingExerciseCount, int? nextUnlockSession)
        {
            if (currentSessionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(currentSessionNumber));
            return !nextUnlockSession.HasValue
                || nextUnlockSession.Value - currentSessionNumber >= GetMinimumGap(knownNonTrackingExerciseCount);
        }
    }
}
