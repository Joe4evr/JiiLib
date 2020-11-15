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
            var listEnt = new DiffList
            {
                Id = listId,
                //KeyComparison = ,
                Entries = new List<DiffListEntryKey>()
            };
            var index = 1;
            foreach (var (key, ov, nv) in list)
            {
                var keyEnt = new DiffListEntryKey
                {
                    Id = Guid.NewGuid().ToString(),
                    KeyName = key,
                    Index = index++,
                    EntryValues = ToDiffListEntries(ov, true, key, listId)
                        .Concat(ToDiffListEntries(nv, false, key, listId))
                        .ToArray()
                };

                listEnt.Entries.Add(keyEnt);
            }

            _context.DiffLists.Add(listEnt);

            await _context.SaveChangesAsync();

            return listId;

            static DiffListEntry[] ToDiffListEntries(DiffValue? dv, bool isOld, string key, string listId)
            {
                return dv switch
                {
                    null => Array.Empty<DiffListEntry>(),
                    { IsSingleValue: true } s => new[]
                    {
                        new DiffListEntry
                        {
                            Id = Guid.NewGuid().ToString(),
                            IsOldValue = isOld,
                            Value = s.Value
                        }
                    },
                    { IsSingleValue: false } m => m.Values.Select(v => new DiffListEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsOldValue = isOld,
                        Value = v
                    }).ToArray()
                };
            }
        }

        public async Task<KeyedDiffList<string>?> GetListAsync(string listId)
        {
            if (listId is null)
                return null;

            var list = await _context.DiffLists
                .Include(d => d.Entries)
                    .ThenInclude(e => e.EntryValues)
                .FirstOrDefaultAsync(d => d.Id == listId);

            if (list is null || list.Entries.Count == 0)
                return null;

            var keys = new List<string>(4);
            var olds = new List<KeyValuePair<string, DiffValue>>(4);
            var news = new List<KeyValuePair<string, DiffValue>>(4);

            foreach (var key in list.Entries.OrderBy(e => e.Index))
            {
                keys.Add(key.KeyName);
                foreach (var val in key.EntryValues.GroupBy(e => e.IsOldValue))
                {
                    var dv = new DiffValue(val.Select(v => v.Value));
                    (val.Key ? olds : news).Add(KeyValuePair.Create(key.KeyName, dv));
                }
            }

            return KeyedDiffList.CreateWithEntries(olds, news,
                comparison: (x, y) => keys.IndexOf(x).CompareTo(keys.IndexOf(y)));
        }

        public async Task<int> GetCountAsync() => await _context.DiffLists.CountAsync();
    }
}
