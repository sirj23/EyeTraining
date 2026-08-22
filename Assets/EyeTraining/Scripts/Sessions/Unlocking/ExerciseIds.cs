using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Unlocking
{
    public static class ExerciseIds
    {
        public const string TrackingHorizontal = "tracking.horizontal";
        public const string TrackingVertical = "tracking.vertical";
        public const string TrackingDiagonalUp = "tracking.diagonal-up";
        public const string TrackingDiagonalDown = "tracking.diagonal-down";
        public const string TrackingCircle = "tracking.circle";
        public const string TrackingHorizontalEllipse = "tracking.horizontal-ellipse";
        public const string TrackingUpperSemicircle = "tracking.upper-semicircle";
        public const string TrackingLowerSemicircle = "tracking.lower-semicircle";
        public const string TrackingUpperHorizontalSemiEllipse = "tracking.upper-horizontal-semi-ellipse";
        public const string TrackingLowerHorizontalSemiEllipse = "tracking.lower-horizontal-semi-ellipse";
        public const string TrackingSquare = "tracking.square";
        public const string TrackingHorizontalRectangle = "tracking.horizontal-rectangle";
        public const string TrackingTriangle = "tracking.triangle";
        public const string TrackingDiamond = "tracking.diamond";
        public const string TrackingHorizontalZigzag = "tracking.horizontal-zigzag";
        public const string TrackingVerticalZigzag = "tracking.vertical-zigzag";
        public const string TrackingHorizontalWave = "tracking.horizontal-wave";
        public const string TrackingVerticalWave = "tracking.vertical-wave";
        public const string TrackingFigureEight = "tracking.figure-eight";
        public const string TrackingSpiral = "tracking.spiral";
        public const string TrackingUShape = "tracking.u-shape";
        public const string TrackingInvertedUShape = "tracking.inverted-u-shape";

        public const string SaccadesNumberJourney = "saccades.number-journey";
        public const string VisualSearchShapeSearch = "visual-search.shape-search";

        private static readonly IReadOnlyList<string> TrackingIds = Array.AsReadOnly(new[]
        {
            TrackingHorizontal,
            TrackingVertical,
            TrackingDiagonalUp,
            TrackingDiagonalDown,
            TrackingCircle,
            TrackingHorizontalEllipse,
            TrackingUpperSemicircle,
            TrackingLowerSemicircle,
            TrackingUpperHorizontalSemiEllipse,
            TrackingLowerHorizontalSemiEllipse,
            TrackingSquare,
            TrackingHorizontalRectangle,
            TrackingTriangle,
            TrackingDiamond,
            TrackingHorizontalZigzag,
            TrackingVerticalZigzag,
            TrackingHorizontalWave,
            TrackingVerticalWave,
            TrackingFigureEight,
            TrackingSpiral,
            TrackingUShape,
            TrackingInvertedUShape
        });

        public static IReadOnlyList<string> AllTracking => TrackingIds;
    }
}
