using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class SessionScheduleResult
    {
        public SessionScheduleResult(
            SessionScheduleStatus status,
            SessionPlan plan,
            IEnumerable<string> newlyUnlockedContentIds,
            IEnumerable<ExerciseFamily> newlyUnlockedFamilies,
            TimeSpan hardDurationLimit,
            int requestedReturningCount,
            int scheduledReturningCount,
            bool wasReturningCountReducedForTime)
        {
            if (!Enum.IsDefined(typeof(SessionScheduleStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (status == SessionScheduleStatus.Success && plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (status != SessionScheduleStatus.Success && plan != null)
            {
                throw new ArgumentException("A failed schedule cannot contain a session plan.", nameof(plan));
            }

            if (newlyUnlockedContentIds == null)
            {
                throw new ArgumentNullException(nameof(newlyUnlockedContentIds));
            }

            if (newlyUnlockedFamilies == null)
            {
                throw new ArgumentNullException(nameof(newlyUnlockedFamilies));
            }

            if (hardDurationLimit <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(hardDurationLimit));
            }

            if (requestedReturningCount < 0 || scheduledReturningCount < 0
                || scheduledReturningCount > requestedReturningCount)
            {
                throw new ArgumentOutOfRangeException(nameof(scheduledReturningCount));
            }

            Status = status;
            Plan = plan;
            NewlyUnlockedContentIds = Copy(newlyUnlockedContentIds);
            NewlyUnlockedFamilies = Copy(newlyUnlockedFamilies);
            HardDurationLimit = hardDurationLimit;
            RequestedReturningCount = requestedReturningCount;
            ScheduledReturningCount = scheduledReturningCount;
            WasReturningCountReducedForTime = wasReturningCountReducedForTime;
        }

        public SessionScheduleStatus Status { get; }

        public bool IsSuccess => Status == SessionScheduleStatus.Success;

        public SessionPlan Plan { get; }

        public IReadOnlyList<string> NewlyUnlockedContentIds { get; }

        public IReadOnlyList<ExerciseFamily> NewlyUnlockedFamilies { get; }

        public TimeSpan HardDurationLimit { get; }

        public int RequestedReturningCount { get; }

        public int ScheduledReturningCount { get; }

        public bool WasReturningCountReducedForTime { get; }

        private static IReadOnlyList<T> Copy<T>(IEnumerable<T> values)
        {
            return new List<T>(values).AsReadOnly();
        }
    }
}
