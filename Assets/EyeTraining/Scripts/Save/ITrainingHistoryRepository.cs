using EyeTraining.Sessions.History;

namespace EyeTraining.Save
{
    public interface ITrainingHistoryRepository
    {
        bool TryLoad(string profileId, out TrainingHistorySnapshot snapshot);

        bool Save(TrainingHistorySnapshot snapshot);

        bool DeleteForProfile(string profileId);
    }
}
