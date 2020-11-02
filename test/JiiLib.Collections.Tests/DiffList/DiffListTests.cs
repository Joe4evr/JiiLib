using System;
using System.Collections.Generic;
using JiiLib.Collections.DiffList;
using Xunit;

namespace JiiLib.Collections.Tests
{
    public class DiffListTests
    {
        public class Factory
        {
            [Fact]
            public void ThrowsWhenGivenNullValues()
            {
                IEnumerable<KeyValuePair<string, DiffValue>> vs = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => KeyedDiffList.CreateWithDualEntries(values: vs));
                Assert.Equal(expected: "values", actual: ex.ParamName);
            }
        }

        public class Ctor
        {

        }
    }
}
