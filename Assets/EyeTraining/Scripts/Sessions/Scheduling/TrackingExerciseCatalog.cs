using System;
using System.Collections.Generic;
using EyeTraining.Exercises;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Scheduling
{
    public sealed class TrackingExerciseCatalog
    {
        private readonly IReadOnlyDictionary<string, TrackingExerciseCatalogEntry> _entries;

        public TrackingExerciseCatalog()
        {
            var entries = new Dictionary<string, TrackingExerciseCatalogEntry>(StringComparer.Ordinal);

            Add(entries, ExerciseIds.TrackingHorizontal, "Poziomo", () => new HorizontalTrackingPath());
            Add(entries, ExerciseIds.TrackingVertical, "Pionowo", () => new VerticalTrackingPath());
            Add(entries, ExerciseIds.TrackingDiagonalUp, "Przekątna w górę", () => new DiagonalUpTrackingPath());
            Add(entries, ExerciseIds.TrackingDiagonalDown, "Przekątna w dół", () => new DiagonalDownTrackingPath());
            Add(entries, ExerciseIds.TrackingCircle, "Okrąg", () => new CircleTrackingPath());
            Add(entries, ExerciseIds.TrackingHorizontalEllipse, "Elipsa pozioma", () => new HorizontalEllipseTrackingPath());
            Add(entries, ExerciseIds.TrackingUpperSemicircle, "Górny półokrąg", () => new UpperSemicircleTrackingPath());
            Add(entries, ExerciseIds.TrackingLowerSemicircle, "Dolny półokrąg", () => new LowerSemicircleTrackingPath());
            Add(entries, ExerciseIds.TrackingUpperHorizontalSemiEllipse, "Górna półelipsa", () => new UpperHorizontalSemiEllipseTrackingPath());
            Add(entries, ExerciseIds.TrackingLowerHorizontalSemiEllipse, "Dolna półelipsa", () => new LowerHorizontalSemiEllipseTrackingPath());
            Add(entries, ExerciseIds.TrackingSquare, "Kwadrat", () => new SquareTrackingPath());
            Add(entries, ExerciseIds.TrackingHorizontalRectangle, "Prostokąt poziomy", () => new HorizontalRectangleTrackingPath());
            Add(entries, ExerciseIds.TrackingTriangle, "Trójkąt", () => new TriangleTrackingPath());
            Add(entries, ExerciseIds.TrackingDiamond, "Diament", () => new DiamondTrackingPath());
            Add(entries, ExerciseIds.TrackingHorizontalZigzag, "Zygzak poziomy", () => new HorizontalZigzagTrackingPath());
            Add(entries, ExerciseIds.TrackingVerticalZigzag, "Zygzak pionowy", () => new VerticalZigzagTrackingPath());
            Add(entries, ExerciseIds.TrackingHorizontalWave, "Fala pozioma", () => new HorizontalWaveTrackingPath());
            Add(entries, ExerciseIds.TrackingVerticalWave, "Fala pionowa", () => new VerticalWaveTrackingPath());
            Add(entries, ExerciseIds.TrackingFigureEight, "Ósemka", () => new FigureEightTrackingPath());
            Add(entries, ExerciseIds.TrackingSpiral, "Spirala", () => new SpiralTrackingPath());
            Add(entries, ExerciseIds.TrackingUShape, "Litera U", () => new UShapeTrackingPath());
            Add(entries, ExerciseIds.TrackingInvertedUShape, "Odwrócona litera U", () => new InvertedUShapeTrackingPath());

            ValidateCompleteness(entries);
            _entries = entries;
        }

        public bool IsTracking(string exerciseId)
        {
            return !string.IsNullOrWhiteSpace(exerciseId) && _entries.ContainsKey(exerciseId);
        }

        public TrackingExerciseCatalogEntry Get(string exerciseId)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId));
            }

            if (!_entries.TryGetValue(exerciseId, out TrackingExerciseCatalogEntry entry))
            {
                throw new KeyNotFoundException($"Unknown Tracking exercise id '{exerciseId}'.");
            }

            return entry;
        }

        private static void Add(
            IDictionary<string, TrackingExerciseCatalogEntry> entries,
            string id,
            string displayName,
            Func<ITrackingPath> pathFactory)
        {
            var definition = new ExerciseDefinition(
                id,
                displayName,
                ExerciseFamily.Tracking,
                ExercisePriority.Normal,
                null,
                false,
                true);

            entries.Add(id, new TrackingExerciseCatalogEntry(definition, pathFactory));
        }

        private static void ValidateCompleteness(
            IReadOnlyDictionary<string, TrackingExerciseCatalogEntry> entries)
        {
            if (entries.Count != ExerciseIds.AllTracking.Count)
            {
                throw new InvalidOperationException("Tracking catalog must contain every Tracking exercise id.");
            }

            for (var index = 0; index < ExerciseIds.AllTracking.Count; index++)
            {
                if (!entries.ContainsKey(ExerciseIds.AllTracking[index]))
                {
                    throw new InvalidOperationException(
                        $"Tracking catalog is missing '{ExerciseIds.AllTracking[index]}'.");
                }
            }
        }
    }
}
