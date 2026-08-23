using System;
using System.Collections.Generic;

namespace EyeTraining.Save
{
    [Serializable]
    internal sealed class TrainingHistoryFileData
    {
        public int version;
        public List<TrainingProfileRecord> profiles;
    }

    [Serializable]
    internal sealed class TrainingProfileRecord
    {
        public string profileId;
        public TrainingStateRecord trainingState;
        public List<ExerciseHistoryRecord> exerciseHistory;
    }

    [Serializable]
    internal sealed class TrainingStateRecord
    {
        public string trainingStartDate;
        public int completedSessionCount;
        public string lastCompletedSessionDate;
    }

    [Serializable]
    internal sealed class ExerciseHistoryRecord
    {
        public string exerciseId;
        public int completedSessionNumber;
        public bool hasAppliedLevel;
        public int appliedLevel;
        public string completionStatus;
        public string feedback;
        public string completedAt;
    }
}
