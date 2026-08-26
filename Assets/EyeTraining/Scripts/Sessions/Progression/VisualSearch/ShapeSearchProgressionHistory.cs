using System;
using System.Collections.Generic;

namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public sealed class ShapeSearchProgressionHistory
    {
        public ShapeSearchProgressionHistory(IEnumerable<ShapeSearchProgressionEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            var copy = new List<ShapeSearchProgressionEntry>(entries);
            var sessions = new HashSet<int>();
            foreach (ShapeSearchProgressionEntry entry in copy)
            {
                if (entry == null) throw new ArgumentException("History cannot contain null entries.", nameof(entries));
                if (!sessions.Add(entry.CompletedSessionNumber))
                    throw new ArgumentException("Shape Search can occur only once per completed session.", nameof(entries));
            }
            copy.Sort((left, right) => left.CompletedSessionNumber.CompareTo(right.CompletedSessionNumber));
            Entries = copy.AsReadOnly();
        }

        public IReadOnlyList<ShapeSearchProgressionEntry> Entries { get; }
    }
}
