using System;
using System.Collections.Generic;
using JiiLib.Collections.DiffList;
using Xunit;

namespace JiiLib.Collections.Tests
{
    public class DiffListTests
    {
        private static KeyedDiffList<T> EmptyTestList<T>()
            where T : notnull
            => KeyedDiffList.CreateWithDualEntries(Array.Empty<KeyValuePair<T, DiffValue>>());

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

        public class Indexer
        {
            [Fact]
            public void ThrowsOnNullKey()
            {
                var list = EmptyTestList<string>();

                var ex = Assert.Throws<ArgumentNullException>(() => list[key: null!]);
                Assert.Equal(expected: "key", actual: ex.ParamName);
            }
        }
    }
}
