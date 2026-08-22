using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class RotationService
    {
        private readonly IExerciseRotationMetadataProvider _metadataProvider;

        public RotationService(IExerciseRotationMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider
                ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        public RotationResult Select(RotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.RequestedCount == 0)
            {
                return new RotationResult(Array.Empty<string>());
            }

            List<RotationCandidate> candidates = BuildCandidates(request);
            candidates.Sort(CompareCandidates);

            var selected = new List<string>();
            var selectedIds = new HashSet<string>(StringComparer.Ordinal);
            var selectedGroups = new HashSet<string>(StringComparer.Ordinal);

            SelectFromPool(
                candidates,
                false,
                request.RequestedCount,
                selected,
                selectedIds,
                selectedGroups);
            SelectFromPool(
                candidates,
                true,
                request.RequestedCount,
                selected,
                selectedIds,
                selectedGroups);

            return new RotationResult(selected);
        }

        private List<RotationCandidate> BuildCandidates(RotationRequest request)
        {
            var newlyUnlocked = new HashSet<string>(
                request.NewlyUnlockedExerciseIds,
                StringComparer.Ordinal);
            var usageByExerciseId = BuildUsageStatistics(request);
            var candidates = new List<RotationCandidate>();

            for (var index = 0; index < request.AvailableExerciseIds.Count; index++)
            {
                string exerciseId = request.AvailableExerciseIds[index];
                if (newlyUnlocked.Contains(exerciseId))
                {
                    continue;
                }

                usageByExerciseId.TryGetValue(exerciseId, out UsageStatistics usage);
                int usageCount = usage?.Count ?? 0;
                int lastUsedSession = usage?.LastUsedSession ?? 0;
                int sessionsSinceLastUse = usageCount == 0
                    ? int.MaxValue
                    : request.CurrentSessionNumber - lastUsedSession;
                bool requiresReinforcement = usageCount == 1
                    && sessionsSinceLastUse >= 1
                    && sessionsSinceLastUse <= 3;

                candidates.Add(new RotationCandidate(
                    exerciseId,
                    ResolveGroupId(exerciseId),
                    usageCount,
                    lastUsedSession,
                    lastUsedSession == request.CurrentSessionNumber - 1,
                    requiresReinforcement));
            }

            return candidates;
        }

        private static Dictionary<string, UsageStatistics> BuildUsageStatistics(RotationRequest request)
        {
            var statistics = new Dictionary<string, UsageStatistics>(StringComparer.Ordinal);

            for (var index = 0; index < request.History.Usages.Count; index++)
            {
                ExerciseUsage usage = request.History.Usages[index];
                if (!statistics.TryGetValue(usage.ExerciseId, out UsageStatistics current))
                {
                    current = new UsageStatistics();
                    statistics.Add(usage.ExerciseId, current);
                }

                current.Count++;
                if (usage.CompletedSessionNumber > current.LastUsedSession)
                {
                    current.LastUsedSession = usage.CompletedSessionNumber;
                }
            }

            return statistics;
        }

        private string ResolveGroupId(string exerciseId)
        {
            if (_metadataProvider.TryGetMetadata(exerciseId, out ExerciseRotationMetadata metadata))
            {
                return metadata.GroupId;
            }

            return "ungrouped:" + exerciseId;
        }

        private static int CompareCandidates(RotationCandidate left, RotationCandidate right)
        {
            int priorityComparison = right.HasUrgentReturn.CompareTo(left.HasUrgentReturn);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            int lastUsedComparison = left.LastUsedSession.CompareTo(right.LastUsedSession);
            if (lastUsedComparison != 0)
            {
                return lastUsedComparison;
            }

            int usageCountComparison = left.UsageCount.CompareTo(right.UsageCount);
            if (usageCountComparison != 0)
            {
                return usageCountComparison;
            }

            return StringComparer.Ordinal.Compare(left.ExerciseId, right.ExerciseId);
        }

        private static void SelectFromPool(
            IReadOnlyList<RotationCandidate> candidates,
            bool usedInPreviousSession,
            int requestedCount,
            List<string> selected,
            HashSet<string> selectedIds,
            HashSet<string> selectedGroups)
        {
            for (var pass = 0; pass < 2 && selected.Count < requestedCount; pass++)
            {
                bool requireNewGroup = pass == 0;

                for (var index = 0; index < candidates.Count && selected.Count < requestedCount; index++)
                {
                    RotationCandidate candidate = candidates[index];
                    if (candidate.UsedInPreviousSession != usedInPreviousSession
                        || selectedIds.Contains(candidate.ExerciseId)
                        || requireNewGroup && selectedGroups.Contains(candidate.GroupId))
                    {
                        continue;
                    }

                    selected.Add(candidate.ExerciseId);
                    selectedIds.Add(candidate.ExerciseId);
                    selectedGroups.Add(candidate.GroupId);
                }
            }
        }

        private sealed class UsageStatistics
        {
            public int Count { get; set; }

            public int LastUsedSession { get; set; }
        }

        private sealed class RotationCandidate
        {
            public RotationCandidate(
                string exerciseId,
                string groupId,
                int usageCount,
                int lastUsedSession,
                bool usedInPreviousSession,
                bool requiresReinforcement)
            {
                ExerciseId = exerciseId;
                GroupId = groupId;
                UsageCount = usageCount;
                LastUsedSession = lastUsedSession;
                UsedInPreviousSession = usedInPreviousSession;
                RequiresReinforcement = requiresReinforcement;
            }

            public string ExerciseId { get; }

            public string GroupId { get; }

            public int UsageCount { get; }

            public int LastUsedSession { get; }

            public bool UsedInPreviousSession { get; }

            public bool RequiresReinforcement { get; }

            public bool HasUrgentReturn => UsageCount == 0 || RequiresReinforcement;
        }
    }
}
