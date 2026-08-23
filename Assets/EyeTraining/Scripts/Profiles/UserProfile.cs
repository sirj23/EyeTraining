using System;
using EyeTraining.Exercises.Landolt;

namespace EyeTraining.Profiles
{
    public sealed class UserProfile
    {
        public UserProfile(string displayName, ProfileCategory category)
            : this(
                Guid.NewGuid().ToString("N"),
                displayName,
                category,
                LandoltBackgroundMode.Dark)
        {
        }

        public UserProfile(
            string id,
            string displayName,
            ProfileCategory category,
            LandoltBackgroundMode landoltBackgroundMode = LandoltBackgroundMode.Dark)
        {
            if (!Enum.IsDefined(typeof(LandoltBackgroundMode), landoltBackgroundMode))
            {
                throw new ArgumentOutOfRangeException(nameof(landoltBackgroundMode));
            }

            Id = id;
            DisplayName = displayName;
            Category = category;
            LandoltBackgroundMode = landoltBackgroundMode;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public ProfileCategory Category { get; }

        public LandoltBackgroundMode LandoltBackgroundMode { get; }
    }
}
