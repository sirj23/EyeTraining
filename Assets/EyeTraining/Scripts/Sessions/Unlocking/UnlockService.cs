using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public sealed class UnlockService
    {
        private readonly UnlockPlan _plan;

        public UnlockService(UnlockPlan plan)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public UnlockState GetState(int completedSessionCount)
        {
            if (completedSessionCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(completedSessionCount),
                    "Completed session count cannot be negative.");
            }

            var unlockedExerciseIds = new List<string>();
            var unlockedFamilies = new List<ExerciseFamily>();
            var newlyUnlockedExerciseIds = new List<string>();
            var newlyUnlockedFamilies = new List<ExerciseFamily>();
            var previousUnlockAtSession = 0;
            UnlockStage nextStage = null;
            var isMilestone = false;

            for (var index = 0; index < _plan.Stages.Count; index++)
            {
                UnlockStage stage = _plan.Stages[index];
                if (stage.RequiredCompletedSessions > completedSessionCount)
                {
                    nextStage = stage;
                    break;
                }

                AddRange(unlockedExerciseIds, stage.ExerciseIds);
                AddRange(unlockedFamilies, stage.Families);
                previousUnlockAtSession = stage.RequiredCompletedSessions;

                if (stage.RequiredCompletedSessions == completedSessionCount)
                {
                    AddRange(newlyUnlockedExerciseIds, stage.ExerciseIds);
                    AddRange(newlyUnlockedFamilies, stage.Families);
                    isMilestone = stage.IsMilestone;
                }
            }

            int? nextUnlockAtSession = nextStage?.RequiredCompletedSessions;
            int? sessionsRemaining = nextUnlockAtSession.HasValue
                ? nextUnlockAtSession.Value - completedSessionCount
                : (int?)null;
            FindMajorUnlockRange(
                completedSessionCount,
                out int previousMajorUnlockAtSession,
                out int? nextMajorUnlockAtSession);
            int? sessionsRemainingToMajorUnlock = nextMajorUnlockAtSession.HasValue
                ? nextMajorUnlockAtSession.Value - completedSessionCount
                : (int?)null;

            return new UnlockState(
                completedSessionCount,
                unlockedExerciseIds.AsReadOnly(),
                unlockedFamilies.AsReadOnly(),
                newlyUnlockedExerciseIds.AsReadOnly(),
                newlyUnlockedFamilies.AsReadOnly(),
                isMilestone,
                previousUnlockAtSession,
                nextUnlockAtSession,
                sessionsRemaining,
                CalculateProgress(completedSessionCount, previousUnlockAtSession, nextUnlockAtSession),
                previousMajorUnlockAtSession,
                nextMajorUnlockAtSession,
                sessionsRemainingToMajorUnlock,
                CalculateProgress(
                    completedSessionCount,
                    previousMajorUnlockAtSession,
                    nextMajorUnlockAtSession));
        }

        private void FindMajorUnlockRange(
            int completedSessionCount,
            out int previousMajorUnlockAtSession,
            out int? nextMajorUnlockAtSession)
        {
            previousMajorUnlockAtSession = 0;
            nextMajorUnlockAtSession = null;

            for (var index = 0; index < _plan.Stages.Count; index++)
            {
                UnlockStage stage = _plan.Stages[index];
                if (stage.Kind != UnlockStageKind.Major)
                {
                    continue;
                }

                if (stage.RequiredCompletedSessions > completedSessionCount)
                {
                    nextMajorUnlockAtSession = stage.RequiredCompletedSessions;
                    return;
                }

                previousMajorUnlockAtSession = stage.RequiredCompletedSessions;
            }
        }

        private static float CalculateProgress(
            int completedSessionCount,
            int previousUnlockAtSession,
            int? nextUnlockAtSession)
        {
            if (!nextUnlockAtSession.HasValue)
            {
                return 1f;
            }

            int interval = nextUnlockAtSession.Value - previousUnlockAtSession;
            if (interval <= 0)
            {
                return 0f;
            }

            return (float)(completedSessionCount - previousUnlockAtSession) / interval;
        }

        private static void AddRange<T>(List<T> target, IReadOnlyList<T> source)
        {
            for (var index = 0; index < source.Count; index++)
            {
                target.Add(source[index]);
            }
        }
    }
}
