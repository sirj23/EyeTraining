using System;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Scheduling
{
    public static class SessionSchedulingDefinitions
    {
        public const string PreparationBasicId = "preparation.basic";
        public const string LandoltStandardId = "landolt.standard";
        public const string SaccadesNumberJourneyId = ExerciseIds.SaccadesNumberJourney;

        public static readonly ExerciseDefinition PreparationBasic = new ExerciseDefinition(
            PreparationBasicId,
            "Rozgrzewka",
            ExerciseFamily.Preparation,
            ExercisePriority.Required,
            TimeSpan.FromMinutes(2),
            false,
            true);

        public static readonly ExerciseDefinition LandoltStandard = new ExerciseDefinition(
            LandoltStandardId,
            "Landolt C",
            ExerciseFamily.LandoltC,
            ExercisePriority.High,
            TimeSpan.FromMinutes(1),
            false,
            true);

        public static readonly ExerciseDefinition SaccadesNumberJourney = new ExerciseDefinition(
            SaccadesNumberJourneyId,
            "Wędrówka wśród liczb",
            ExerciseFamily.Saccades,
            ExercisePriority.Required,
            TimeSpan.FromSeconds(15),
            false,
            true);
    }
}
