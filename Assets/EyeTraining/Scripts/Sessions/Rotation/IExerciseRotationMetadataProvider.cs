namespace EyeTraining.Sessions.Rotation
{
    public interface IExerciseRotationMetadataProvider
    {
        bool TryGetMetadata(string exerciseId, out ExerciseRotationMetadata metadata);
    }
}
