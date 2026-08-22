using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public sealed class UnlockStage
    {
        public UnlockStage(
            int requiredCompletedSessions,
            IEnumerable<string> exerciseIds = null,
            IEnumerable<ExerciseFamily> families = null,
            UnlockStageKind kind = UnlockStageKind.Regular)
        {
            if (requiredCompletedSessions <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCompletedSessions),
                    "Required completed sessions must be positive.");
            }

            if (!Enum.IsDefined(typeof(UnlockStageKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            RequiredCompletedSessions = requiredCompletedSessions;
            ExerciseIds = CopyExerciseIds(exerciseIds);
            Families = CopyFamilies(families);
            Kind = kind;
        }

        public int RequiredCompletedSessions { get; }

        public IReadOnlyList<string> ExerciseIds { get; }

        public IReadOnlyList<ExerciseFamily> Families { get; }

        public UnlockStageKind Kind { get; }

        public bool IsMilestone => Kind == UnlockStageKind.Major;

        private static IReadOnlyList<string> CopyExerciseIds(IEnumerable<string> exerciseIds)
        {
            var copy = exerciseIds == null
                ? new List<string>()
                : new List<string>(exerciseIds);

            for (var index = 0; index < copy.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(copy[index]))
                {
                    throw new ArgumentException("Exercise ids cannot be null or empty.", nameof(exerciseIds));
                }
            }

            return copy.AsReadOnly();
        }

        private static IReadOnlyList<ExerciseFamily> CopyFamilies(IEnumerable<ExerciseFamily> families)
        {
            var copy = families == null
                ? new List<ExerciseFamily>()
                : new List<ExerciseFamily>(families);

            for (var index = 0; index < copy.Count; index++)
            {
                if (!Enum.IsDefined(typeof(ExerciseFamily), copy[index]))
                {
                    throw new ArgumentOutOfRangeException(nameof(families));
                }
            }

            return copy.AsReadOnly();
        }
    }
}
