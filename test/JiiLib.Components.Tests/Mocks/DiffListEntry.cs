using System;
using System.Collections.Generic;
using System.Text;

namespace JiiLib.Components.Tests
{
#nullable disable warnings
    public sealed class DiffList
    {
        public string Id { get; set; }
        //public StringComparison KeyComparison { get; set; }
        public ICollection<DiffListEntryKey> Entries { get; set; }
    }

    public sealed class DiffListEntryKey
    {
        public string Id { get; set; }
        public string KeyName { get; set; }
        public int Index { get; set; }
        public ICollection<DiffListEntry> EntryValues { get; set; }
    }

    public sealed class DiffListEntry
    {
        public string Id { get; set; }

        public string Value { get; set; }
        public bool IsOldValue { get; set; }
    }
}
