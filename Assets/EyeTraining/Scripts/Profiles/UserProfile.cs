using System;

namespace EyeTraining.Profiles
{
    public sealed class UserProfile
    {
        public UserProfile(string displayName, ProfileCategory category)
        {
            Id = Guid.NewGuid().ToString("N");
            DisplayName = displayName;
            Category = category;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public ProfileCategory Category { get; }
    }
}
