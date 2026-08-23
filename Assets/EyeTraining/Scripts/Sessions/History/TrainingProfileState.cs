using System;

namespace EyeTraining.Sessions.History
{
    public sealed class TrainingProfileState
    {
        public TrainingProfileState(
            string profileId,
            DateTimeOffset? trainingStartDate,
            int completedSessionCount,
            DateTimeOffset? lastCompletedSessionDate)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException("Profile id cannot be empty.", nameof(profileId));
            }

            if (completedSessionCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionCount));
            }

            ValidateDate(trainingStartDate, nameof(trainingStartDate));
            ValidateDate(lastCompletedSessionDate, nameof(lastCompletedSessionDate));

            if (!trainingStartDate.HasValue
                && (completedSessionCount != 0 || lastCompletedSessionDate.HasValue))
            {
                throw new ArgumentException(
                    "A profile without a training start date cannot contain completed sessions.");
            }

            if (completedSessionCount == 0 && lastCompletedSessionDate.HasValue)
            {
                throw new ArgumentException(
                    "A profile without completed sessions cannot have a last completion date.");
            }

            if (completedSessionCount > 0 && !lastCompletedSessionDate.HasValue)
            {
                throw new ArgumentException(
                    "A profile with completed sessions must have a last completion date.");
            }

            if (trainingStartDate.HasValue
                && lastCompletedSessionDate.HasValue
                && lastCompletedSessionDate.Value < trainingStartDate.Value)
            {
                throw new ArgumentException(
                    "Last completed session date cannot be earlier than training start date.");
            }

            ProfileId = profileId;
            TrainingStartDate = trainingStartDate;
            CompletedSessionCount = completedSessionCount;
            LastCompletedSessionDate = lastCompletedSessionDate;
        }

        public string ProfileId { get; }

        public DateTimeOffset? TrainingStartDate { get; }

        public int CompletedSessionCount { get; }

        public DateTimeOffset? LastCompletedSessionDate { get; }

        public static TrainingProfileState CreateNotStarted(string profileId)
        {
            return new TrainingProfileState(profileId, null, 0, null);
        }

        private static void ValidateDate(DateTimeOffset? value, string parameterName)
        {
            if (value.HasValue && value.Value == default)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
