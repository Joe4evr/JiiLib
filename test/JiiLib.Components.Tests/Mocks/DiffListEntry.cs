using System;
using System.Collections.Generic;
using System.Text;

namespace JiiLib.Components.Tests
{
#nullable disable warnings
    public sealed class DiffListEntry
    {
        public string Id { get; set; }
        public string ListId { get; set; }

        public string EntryKey { get; set; }
        public string Value { get; set; }
        public bool IsOldValue { get; set; }
    }
}
