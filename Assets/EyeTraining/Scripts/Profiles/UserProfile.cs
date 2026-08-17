using System;

namespace EyeTraining.Profiles
{
    public sealed class UserProfile
    {
        public UserProfile(string displayName, ProfileCategory category)
            : this(Guid.NewGuid().ToString("N"), displayName, category)
        {
        }

        public UserProfile(string id, string displayName, ProfileCategory category)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public ProfileCategory Category { get; }
    }
}
