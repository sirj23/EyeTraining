using System;

namespace EyeTraining.Sessions.History
{
    public static class TrainingDayCalculator
    {
        public static int Calculate(
            DateTimeOffset? trainingStartDate,
            DateTimeOffset currentDate)
        {
            if (currentDate == default)
            {
                throw new ArgumentOutOfRangeException(nameof(currentDate));
            }

            if (!trainingStartDate.HasValue)
            {
                return 1;
            }

            DateTime localStartDate = trainingStartDate.Value.ToLocalTime().Date;
            DateTime localCurrentDate = currentDate.ToLocalTime().Date;
            int elapsedCalendarDays = (localCurrentDate - localStartDate).Days;
            return Math.Max(1, elapsedCalendarDays + 1);
        }
    }
}
