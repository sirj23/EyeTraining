using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public sealed class UnlockState
    {
        internal UnlockState(
            int currentCompletedSessions,
            IReadOnlyList<string> unlockedExerciseIds,
            IReadOnlyList<ExerciseFamily> unlockedFamilies,
            IReadOnlyList<string> newlyUnlockedExerciseIds,
            IReadOnlyList<ExerciseFamily> newlyUnlockedFamilies,
            bool isMilestone,
            int previousUnlockAtSession,
            int? nextUnlockAtSession,
            int? sessionsRemaining,
            float progress01,
            int previousMajorUnlockAtSession,
            int? nextMajorUnlockAtSession,
            int? sessionsRemainingToMajorUnlock,
            float majorUnlockProgress01)
        {
            CurrentCompletedSessions = currentCompletedSessions;
            UnlockedExerciseIds = unlockedExerciseIds;
            UnlockedFamilies = unlockedFamilies;
            NewlyUnlockedExerciseIds = newlyUnlockedExerciseIds;
            NewlyUnlockedFamilies = newlyUnlockedFamilies;
            IsMilestone = isMilestone;
            PreviousUnlockAtSession = previousUnlockAtSession;
            NextUnlockAtSession = nextUnlockAtSession;
            SessionsRemaining = sessionsRemaining;
            Progress01 = progress01;
            PreviousMajorUnlockAtSession = previousMajorUnlockAtSession;
            NextMajorUnlockAtSession = nextMajorUnlockAtSession;
            SessionsRemainingToMajorUnlock = sessionsRemainingToMajorUnlock;
            MajorUnlockProgress01 = majorUnlockProgress01;
        }

        public int CurrentCompletedSessions { get; }

        public IReadOnlyList<string> UnlockedExerciseIds { get; }

        public IReadOnlyList<ExerciseFamily> UnlockedFamilies { get; }

        public IReadOnlyList<string> NewlyUnlockedExerciseIds { get; }

        public IReadOnlyList<ExerciseFamily> NewlyUnlockedFamilies { get; }

        public bool IsMilestone { get; }

        public int PreviousUnlockAtSession { get; }

        public bool HasNextUnlock => NextUnlockAtSession.HasValue;

        public int? NextUnlockAtSession { get; }

        public int? SessionsRemaining { get; }

        public float Progress01 { get; }

        public int PreviousMajorUnlockAtSession { get; }

        public bool HasNextMajorUnlock => NextMajorUnlockAtSession.HasValue;

        public int? NextMajorUnlockAtSession { get; }

        public int? SessionsRemainingToMajorUnlock { get; }

        public float MajorUnlockProgress01 { get; }
    }
}
