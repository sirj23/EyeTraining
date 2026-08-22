using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public sealed class UnlockPlan
    {
        public UnlockPlan(IEnumerable<UnlockStage> stages)
        {
            if (stages == null)
            {
                throw new ArgumentNullException(nameof(stages));
            }

            var orderedStages = new List<UnlockStage>(stages);
            Validate(orderedStages);
            orderedStages.Sort((left, right) =>
                left.RequiredCompletedSessions.CompareTo(right.RequiredCompletedSessions));
            Stages = orderedStages.AsReadOnly();
        }

        public IReadOnlyList<UnlockStage> Stages { get; }

        private static void Validate(IReadOnlyList<UnlockStage> stages)
        {
            var thresholds = new HashSet<int>();
            var exerciseIds = new HashSet<string>(StringComparer.Ordinal);
            var families = new HashSet<ExerciseFamily>();

            for (var stageIndex = 0; stageIndex < stages.Count; stageIndex++)
            {
                UnlockStage stage = stages[stageIndex];
                if (stage == null)
                {
                    throw new ArgumentException("Unlock plan cannot contain null stages.", nameof(stages));
                }

                if (!thresholds.Add(stage.RequiredCompletedSessions))
                {
                    throw new ArgumentException("Unlock stage thresholds must be unique.", nameof(stages));
                }

                for (var exerciseIndex = 0; exerciseIndex < stage.ExerciseIds.Count; exerciseIndex++)
                {
                    if (!exerciseIds.Add(stage.ExerciseIds[exerciseIndex]))
                    {
                        throw new ArgumentException(
                            "An exercise id can be unlocked only once.",
                            nameof(stages));
                    }
                }

                for (var familyIndex = 0; familyIndex < stage.Families.Count; familyIndex++)
                {
                    if (!families.Add(stage.Families[familyIndex]))
                    {
                        throw new ArgumentException(
                            "An exercise family can be unlocked only once.",
                            nameof(stages));
                    }
                }
            }
        }
    }
}
