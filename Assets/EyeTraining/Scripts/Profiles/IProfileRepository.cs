using System.Collections.Generic;

namespace EyeTraining.Profiles
{
    public interface IProfileRepository
    {
        IReadOnlyList<UserProfile> GetAll();

        bool Save(UserProfile profile);
    }
}
