using System;
using System.Collections.Generic;
using EyeTraining.Sessions.Unlocking;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class TrackingRotationCatalog : IExerciseRotationMetadataProvider
    {
        private const string GroupPrefix = "tracking.";

        private readonly Dictionary<string, ExerciseRotationMetadata> _metadataByExerciseId;

        public TrackingRotationCatalog()
        {
            _metadataByExerciseId = new Dictionary<string, ExerciseRotationMetadata>(StringComparer.Ordinal);

            Add(TrackingRotationGroup.Linear,
                ExerciseIds.TrackingHorizontal,
                ExerciseIds.TrackingVertical);
            Add(TrackingRotationGroup.Diagonal,
                ExerciseIds.TrackingDiagonalUp,
                ExerciseIds.TrackingDiagonalDown);
            Add(TrackingRotationGroup.Circular,
                ExerciseIds.TrackingCircle,
                ExerciseIds.TrackingHorizontalEllipse);
            Add(TrackingRotationGroup.Arc,
                ExerciseIds.TrackingUpperSemicircle,
                ExerciseIds.TrackingLowerSemicircle,
                ExerciseIds.TrackingUpperHorizontalSemiEllipse,
                ExerciseIds.TrackingLowerHorizontalSemiEllipse,
                ExerciseIds.TrackingUShape,
                ExerciseIds.TrackingInvertedUShape);
            Add(TrackingRotationGroup.Polygon,
                ExerciseIds.TrackingSquare,
                ExerciseIds.TrackingHorizontalRectangle,
                ExerciseIds.TrackingTriangle,
                ExerciseIds.TrackingDiamond);
            Add(TrackingRotationGroup.Zigzag,
                ExerciseIds.TrackingHorizontalZigzag,
                ExerciseIds.TrackingVerticalZigzag);
            Add(TrackingRotationGroup.Wave,
                ExerciseIds.TrackingHorizontalWave,
                ExerciseIds.TrackingVerticalWave);
            Add(TrackingRotationGroup.Complex,
                ExerciseIds.TrackingFigureEight,
                ExerciseIds.TrackingSpiral);
        }

        public bool TryGetMetadata(string exerciseId, out ExerciseRotationMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
            {
                metadata = null;
                return false;
            }

            return _metadataByExerciseId.TryGetValue(exerciseId, out metadata);
        }

        private void Add(TrackingRotationGroup group, params string[] exerciseIds)
        {
            string groupId = GroupPrefix + group.ToString().ToLowerInvariant();

            for (var index = 0; index < exerciseIds.Length; index++)
            {
                string exerciseId = exerciseIds[index];
                _metadataByExerciseId.Add(
                    exerciseId,
                    new ExerciseRotationMetadata(exerciseId, groupId));
            }
        }
    }
}
