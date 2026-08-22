using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class DefaultLandoltSchedulePolicy : ILandoltSchedulePolicy
    {
        private static readonly HashSet<int> ScheduledSessions = new HashSet<int>
        {
            1, 3, 5, 7, 8, 10, 12, 14
        };

        public bool ShouldSchedule(int sessionNumber)
        {
            if (sessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionNumber));
            }

            return ScheduledSessions.Contains(sessionNumber);
        }
    }
}
