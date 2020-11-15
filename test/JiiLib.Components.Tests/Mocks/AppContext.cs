using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JiiLib.Components.Tests
{
#nullable disable warnings
    internal sealed class AppContext : DbContext
    {
        public DbSet<DiffList> DiffLists { get; set; }
        public DbSet<DiffListEntryKey> DiffListKeys { get; set; }
        public DbSet<DiffListEntry> DiffListEntries { get; set; }


        public AppContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
