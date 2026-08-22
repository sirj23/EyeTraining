using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Rotation
{
    public sealed class RotationResult
    {
        internal RotationResult(IEnumerable<string> selectedExerciseIds)
        {
            if (selectedExerciseIds == null)
            {
                throw new ArgumentNullException(nameof(selectedExerciseIds));
            }

            SelectedExerciseIds = new List<string>(selectedExerciseIds).AsReadOnly();
        }

        public IReadOnlyList<string> SelectedExerciseIds { get; }
    }
}
