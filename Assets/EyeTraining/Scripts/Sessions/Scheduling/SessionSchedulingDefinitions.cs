using System;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Scheduling
{
    public static class SessionSchedulingDefinitions
    {
        public const string PreparationBasicId = "preparation.basic";
        public const string LandoltStandardId = "landolt.standard";
        public const string SaccadesNumberJourneyId = ExerciseIds.SaccadesNumberJourney;
        public const string VisualSearchShapeSearchId = ExerciseIds.VisualSearchShapeSearch;
        public const string PeripheralEdgeSignalsId = ExerciseIds.PeripheralEdgeSignals;

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

        public static readonly ExerciseDefinition VisualSearchShapeSearch = new ExerciseDefinition(
            VisualSearchShapeSearchId,
            "Znajdź kształt",
            ExerciseFamily.VisualSearch,
            ExercisePriority.Required,
            TimeSpan.FromSeconds(30),
            false,
            true);

        public static readonly ExerciseDefinition SaccadesNumberJourneyReturning = WithPriority(
            SaccadesNumberJourney, ExercisePriority.Normal);
        public static readonly ExerciseDefinition VisualSearchShapeSearchReturning = WithPriority(
            VisualSearchShapeSearch, ExercisePriority.Normal);
        public static readonly ExerciseDefinition PeripheralEdgeSignals = new ExerciseDefinition(
            PeripheralEdgeSignalsId,
            "Sygnały na obrzeżach",
            ExerciseFamily.Peripheral,
            ExercisePriority.Required,
            null,
            false,
            true);
        public static readonly ExerciseDefinition PeripheralEdgeSignalsReturning = WithPriority(
            PeripheralEdgeSignals, ExercisePriority.Normal);

        private static ExerciseDefinition WithPriority(ExerciseDefinition source, ExercisePriority priority) =>
            new(source.Id, source.DisplayName, source.Family, priority, source.EstimatedDuration,
                source.RequiresBreakAfter, source.CanAppearInMilestoneSession);
    }
}
