using System;
using System.Collections.Generic;
using System.Linq;

namespace EyeTraining.Sessions.Progression.Peripheral
{
    public sealed class EdgeSignalsProgressionHistory
    {
        public EdgeSignalsProgressionHistory(IEnumerable<EdgeSignalsProgressionEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            var copy = entries.OrderBy(x => x.CompletedSessionNumber).ToArray();
            if (copy.Any(x => x == null)) throw new ArgumentException("History contains null.");
            for (var i = 1; i < copy.Length; i++)
                if (copy[i - 1].CompletedSessionNumber == copy[i].CompletedSessionNumber)
                    throw new ArgumentException("History contains duplicate session.");
            Entries = Array.AsReadOnly(copy);
        }
        public IReadOnlyList<EdgeSignalsProgressionEntry> Entries { get; }
    }
}
