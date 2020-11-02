using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JiiLib.Collections;
using JiiLib.Collections.DiffList;

namespace JiiLib.Components.Tests
{
    internal sealed class DiffListRepository : IDiffListRepository
    {
        private readonly AppContext _context;

        public DiffListRepository(AppContext context)
        {
            _context = context;
        }

        public async Task<string> AddListAsync(KeyedDiffList<string> list)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));


            var listId = Guid.NewGuid().ToString();
            foreach (var (key, ov, nv) in list)
            {
                var oe = ov switch
                {
                    null => Array.Empty<DiffListEntry>(),
                    { IsSingleValue: true  } s => new[]
                    {
                        new DiffListEntry
                        {
                            Id = Guid.NewGuid().ToString(),
                            ListId = listId,
                            EntryKey = key,
                            IsOldValue = true,
                            Value = s.Value
                        }
                    },
                    { IsSingleValue: false } m => m.Values.Select(v => new DiffListEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        ListId = listId,
                        EntryKey = key,
                        IsOldValue = true,
                        Value = v
                    }).ToArray()
                };
                if (oe.Length > 0)
                {
                    await _context.DiffListEntries.AddRangeAsync(oe);
                }


                var ne = nv switch
                {
                    null => Array.Empty<DiffListEntry>(),
                    { IsSingleValue: true  } s => new[]
                    {
                        new DiffListEntry
                        {
                            Id = Guid.NewGuid().ToString(),
                            ListId = listId,
                            EntryKey = key,
                            IsOldValue = false,
                            Value = s.Value
                        }
                    },
                    { IsSingleValue: false } m => m.Values.Select(v => new DiffListEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        ListId = listId,
                        EntryKey = key,
                        IsOldValue = false,
                        Value = v
                    }).ToArray()
                };
                if (ne.Length > 0)
                {
                    await _context.DiffListEntries.AddRangeAsync(ne);
                }
            }

            await _context.SaveChangesAsync();

            return listId;
        }

        public async Task<KeyedDiffList<string>?> GetListAsync(string id, Comparison<string>? comparison = null)
        {
            var entries = await _context.DiffListEntries
                .Where(l => l.ListId == id)
                .ToArrayAsync();

            if (entries.Length == 0)
                return null;

            var split = entries.ToLookup(e => e.IsOldValue);
            var olds = new List<KeyValuePair<string, DiffValue>>(4);
            var news = new List<KeyValuePair<string, DiffValue>>(4);

            foreach (var entry in split[true].GroupBy(e => e.EntryKey))
            {
                olds.Add(KeyValuePair.Create(entry.Key, new DiffValue(entry.Select(e => e.Value))));
            }
            foreach (var entry in split[false].GroupBy(e => e.EntryKey))
            {
                news.Add(KeyValuePair.Create(entry.Key, new DiffValue(entry.Select(e => e.Value))));
            }

            return KeyedDiffList.CreateWithEntries(olds, news, StringComparer.OrdinalIgnoreCase);
        }
    }
}
