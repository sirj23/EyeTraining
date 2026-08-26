using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.Saccades
{
    public sealed class NumberJourneyProgressionHistory
    {
        public NumberJourneyProgressionHistory(IEnumerable<NumberJourneyProgressionEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var copy = new List<NumberJourneyProgressionEntry>(entries);
            var sessions = new HashSet<int>();
            for (var index = 0; index < copy.Count; index++)
            {
                NumberJourneyProgressionEntry entry = copy[index];
                if (entry == null)
                {
                    throw new ArgumentException("History cannot contain null entries.", nameof(entries));
                }

                if (!sessions.Add(entry.CompletedSessionNumber))
                {
                    throw new ArgumentException(
                        "Number Journey can occur only once per completed session.",
                        nameof(entries));
                }
            }

            copy.Sort((left, right) =>
                left.CompletedSessionNumber.CompareTo(right.CompletedSessionNumber));
            Entries = copy.AsReadOnly();
        }

        public IReadOnlyList<NumberJourneyProgressionEntry> Entries { get; }
    }
}
