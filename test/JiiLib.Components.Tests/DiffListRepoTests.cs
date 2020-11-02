using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JiiLib.Collections;
using JiiLib.Collections.DiffList;
using Xunit;

namespace JiiLib.Components.Tests
{
    public class DiffListRepoTests
    {
        private static IServiceProvider Setup()
        {
            var svc = new ServiceCollection();
            svc.AddDbContext<AppContext>(options =>
            {
                options.UseInMemoryDatabase("TestingDatabase");
            });
            svc.AddScoped<IDiffListRepository, DiffListRepository>();

            return svc.BuildServiceProvider();
        }

        private readonly IServiceProvider _services = Setup();

        [Fact]
        public async Task VerifyListsCanRoundTrip()
        {
            var testList = KeyedDiffList.CreateWithDualEntries(
                values: new[]
                {
                    KeyValuePair.Create("CPU", new DiffValue("AMD Ryzen 3200G")),
                    KeyValuePair.Create("RAM", new DiffValue("4x 16GB G.Skill Sniper X F4-3600C19Q-64GSXKB")),
                    KeyValuePair.Create("GPU", new DiffValue("Gigabyte GeForce GTX 1070")),
                    KeyValuePair.Create("Storage", new DiffValue(new[]
                    {
                        "Samsung NVMe 970 EVO Plus (250GB)",
                        "Samsung SATA 860 EVO (500GB)"
                    }))
                })
                .SetEntry("CPU", "AMD Ryzen 5600")
                .AddTo("Storage", new[]
                {
                    "Seagate Barracuda Compute (6TB)",
                    "Crucial BX500 (2TB)"
                });

            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDiffListRepository>();
            var id = await repo.AddListAsync(testList);

            var roundTripped = await repo.GetListAsync(id);

            Assert.NotNull(roundTripped);
            Assert.Equal(expected: testList, actual: roundTripped,
                comparer: DiffListComparer<string>.Instance);
        }
    }
}
