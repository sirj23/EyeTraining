using System;
using System.Collections.Generic;

namespace EyeTraining.Exercises.VisualSearch
{
    public enum ShapeSearchSelectionOutcome
    {
        Correct,
        Incorrect,
        AlreadySelected,
        RoundCompleted
    }

    public sealed class ShapeSearchProgress
    {
        private readonly ShapeSearchRound _round;
        private readonly HashSet<int> _selectedTargets = new();

        public ShapeSearchProgress(ShapeSearchRound round)
        {
            _round = round ?? throw new ArgumentNullException(nameof(round));
        }

        public int CorrectSelections => _selectedTargets.Count;

        public int ErrorCount { get; private set; }

        public bool IsCompleted => CorrectSelections == _round.TargetCount;

        public bool IsTargetSelected(int itemIndex) => _selectedTargets.Contains(itemIndex);

        public ShapeSearchSelectionOutcome Select(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _round.Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex));
            }

            if (IsCompleted)
            {
                return ShapeSearchSelectionOutcome.RoundCompleted;
            }

            ShapeSearchRoundItem item = _round.Items[itemIndex];
            if (!item.IsTarget)
            {
                ErrorCount++;
                return ShapeSearchSelectionOutcome.Incorrect;
            }

            if (!_selectedTargets.Add(itemIndex))
            {
                return ShapeSearchSelectionOutcome.AlreadySelected;
            }

            return IsCompleted
                ? ShapeSearchSelectionOutcome.RoundCompleted
                : ShapeSearchSelectionOutcome.Correct;
        }
    }
}
