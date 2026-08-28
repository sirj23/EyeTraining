using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public sealed class MajorUnlockPacePolicy
    {
        private readonly HashSet<string> relevantExerciseIds;

        public MajorUnlockPacePolicy(IEnumerable<string> relevantExerciseIds)
        {
            if (relevantExerciseIds == null) throw new ArgumentNullException(nameof(relevantExerciseIds));
            this.relevantExerciseIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in relevantExerciseIds)
                if (string.IsNullOrWhiteSpace(id) || !this.relevantExerciseIds.Add(id))
                    throw new ArgumentException("Paced exercise IDs must be unique and non-empty.", nameof(relevantExerciseIds));
        }

        public int GetMinimumGapForNextUnlock(int alreadyUnlockedExerciseCount)
        {
            if (alreadyUnlockedExerciseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(alreadyUnlockedExerciseCount));
            return alreadyUnlockedExerciseCount < 5 ? 2 : 3;
        }

        public int GetEarliestNextUnlockSession(int previousUnlockSession, int alreadyUnlockedExerciseCount)
        {
            if (previousUnlockSession <= 0) throw new ArgumentOutOfRangeException(nameof(previousUnlockSession));
            return previousUnlockSession + GetMinimumGapForNextUnlock(alreadyUnlockedExerciseCount);
        }

        public void Validate(UnlockPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var unlockedCount = 0;
            var previousSession = 0;
            foreach (UnlockStage stage in plan.Stages)
            {
                var relevantCount = 0;
                foreach (string id in stage.ExerciseIds)
                    if (relevantExerciseIds.Contains(id)) relevantCount++;
                if (relevantCount == 0) continue;
                if (relevantCount > 1)
                    throw new ArgumentException("A stage cannot unlock more than one non-Tracking exercise.", nameof(plan));
                if (stage.Kind != UnlockStageKind.Major)
                    throw new ArgumentException("A paced non-Tracking exercise must use a major unlock stage.", nameof(plan));
                if (unlockedCount > 0)
                {
                    int requiredGap = GetMinimumGapForNextUnlock(unlockedCount);
                    if (stage.RequiredCompletedSessions - previousSession < requiredGap)
                        throw new ArgumentException("Non-Tracking major unlocks violate the minimum pace.", nameof(plan));
                }
                previousSession = stage.RequiredCompletedSessions;
                unlockedCount++;
            }
        }

        public bool IsRelevantExercise(string exerciseId) =>
            !string.IsNullOrWhiteSpace(exerciseId) && relevantExerciseIds.Contains(exerciseId);
    }
}
