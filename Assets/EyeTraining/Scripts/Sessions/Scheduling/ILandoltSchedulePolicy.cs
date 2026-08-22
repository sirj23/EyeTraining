namespace EyeTraining.Sessions.Scheduling
{
    public interface ILandoltSchedulePolicy
    {
        bool ShouldSchedule(int sessionNumber);
    }
}
