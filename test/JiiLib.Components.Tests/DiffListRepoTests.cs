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
            var testList = KeyedDiffList.CreateWithEntries(
                oldValues: new[]
                {
                    KeyValuePair.Create("CPU", new DiffValue("AMD Ryzen 3200G")),
                    KeyValuePair.Create("GPU", new DiffValue("Gigabyte GTX 1070"))
                },
                newValues: new[]
                {
                    KeyValuePair.Create("CPU", new DiffValue("AMD Ryzen 5600"))
                });

            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDiffListRepository>();
            var id = await repo.AddListAsync(testList);

            var roundTripped = await repo.GetListAsync(id);

            Assert.Equal(expected: testList, actual: roundTripped,
                comparer: DiffListComparer<string>.Instance);
        }
    }
}
