namespace EyeTraining.Sessions.Unlocking
{
    public static class DefaultUnlockPlan
    {
        public static UnlockPlan Create()
        {
            return new UnlockPlan(new[]
            {
                new UnlockStage(
                    1,
                    new[]
                    {
                        ExerciseIds.TrackingHorizontal,
                        ExerciseIds.TrackingVertical,
                        ExerciseIds.TrackingDiagonalUp,
                        ExerciseIds.TrackingCircle,
                        ExerciseIds.TrackingHorizontalEllipse
                    },
                    new[] { ExerciseFamily.Tracking }),
                new UnlockStage(
                    2,
                    new[]
                    {
                        ExerciseIds.TrackingDiagonalDown,
                        ExerciseIds.TrackingUpperSemicircle
                    }),
                new UnlockStage(3, new[] { ExerciseIds.TrackingLowerSemicircle }),
                new UnlockStage(
                    4,
                    new[]
                    {
                        ExerciseIds.TrackingSquare,
                        ExerciseIds.TrackingTriangle
                    }),
                new UnlockStage(5, new[] { ExerciseIds.TrackingDiamond }),
                new UnlockStage(
                    6,
                    new[]
                    {
                        ExerciseIds.TrackingHorizontalRectangle,
                        ExerciseIds.TrackingUpperHorizontalSemiEllipse
                    }),
                new UnlockStage(
                    7,
                    new[] { ExerciseIds.SaccadesNumberJourney },
                    new[] { ExerciseFamily.Saccades },
                    UnlockStageKind.Major),
                new UnlockStage(8, new[] { ExerciseIds.TrackingLowerHorizontalSemiEllipse }),
                new UnlockStage(
                    9,
                    new[]
                    {
                        ExerciseIds.TrackingHorizontalZigzag,
                        ExerciseIds.TrackingVerticalZigzag
                    }),
                new UnlockStage(10, new[] { ExerciseIds.TrackingHorizontalWave }),
                new UnlockStage(
                    11,
                    new[]
                    {
                        ExerciseIds.TrackingVerticalWave,
                        ExerciseIds.TrackingUShape
                    }),
                new UnlockStage(12, new[] { ExerciseIds.TrackingInvertedUShape }),
                new UnlockStage(
                    13,
                    new[]
                    {
                        ExerciseIds.TrackingFigureEight,
                        ExerciseIds.TrackingSpiral
                    }),
                new UnlockStage(
                    14,
                    new[] { ExerciseIds.VisualSearchShapeSearch },
                    new[] { ExerciseFamily.VisualSearch },
                    UnlockStageKind.Major)
            });
        }
    }
}
