using System;
using System.Collections.Generic;
using JiiLib.Collections.DiffList;
using Xunit;

namespace JiiLib.Collections.Tests
{
    public class DiffValueTests
    {
        public class CtorSingle
        {
            [Fact]
            public void ThrowsWhenGivenNullArg()
            {
                string arg = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => new DiffValue(value: arg));
                Assert.Equal(expected: "value", actual: ex.ParamName);
            }

            [Fact]
            public void ThrowsWhenGivenEmptyString()
            {
                string arg = String.Empty;

                var ex = Assert.Throws<ArgumentException>(() => new DiffValue(value: arg));
                Assert.Equal(expected: "value", actual: ex.ParamName);
                Assert.Equal(expected: "Value may not be empty string. (Parameter 'value')", actual: ex.Message);
            }
        }

        public class CtorMultiple
        {
            [Fact]
            public void ThrowsWhenGivenNullArg()
            {
                IEnumerable<string> args = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => new DiffValue(values: args));
                Assert.Equal(expected: "values", actual: ex.ParamName);
            }

            [Fact]
            public void ThrowsWhenGivenEmptyEnumerable()
            {
                IEnumerable<string> args = Array.Empty<string>();

                var ex = Assert.Throws<ArgumentException>(() => new DiffValue(values: args));
                Assert.Equal(expected: "values", actual: ex.ParamName);
                Assert.Equal(expected: "Values may not be empty. (Parameter 'values')", actual: ex.Message);
            }

            [Fact]
            public void CreatesSingleValueWithSingleElementEnumerable()
            {
                IEnumerable<string> args = new[] { "Test" };

                var dv = new DiffValue(args);

                Assert.True(dv.IsSingleValue);
            }
        }

        public class IsSingleValue
        {
            [Fact]
            public void ThrowsOnSingleWhenMultiValue()
            {
                var dv = new DiffValue(new[] { "Test1", "Test2" });

                Assert.False(dv.IsSingleValue);
                var ex = Assert.Throws<InvalidOperationException>(() => dv.Value);
                Assert.Equal(expected: "DiffValue instance was not single-value.", actual: ex.Message);
            }

            [Fact]
            public void ThrowsOnMultiWhenSingleValue()
            {
                var dv = new DiffValue("Test");

                Assert.True(dv.IsSingleValue);
                var ex = Assert.Throws<InvalidOperationException>(() => dv.Values);
                Assert.Equal(expected: "DiffValue instance was not multi-value.", actual: ex.Message);
            }
        }

        public class AddSingle
        {
            [Fact]
            public void ReturnsSameWhenGivenNullArg()
            {
                var dv1 = new DiffValue("Test1");
                string arg = null!;

                var dv2 = dv1.Add(value: arg);

                Assert.Same(expected: dv1, actual: dv2);
            }

            [Fact]
            public void ReturnsSameWhenGivenEmptyString()
            {
                var dv1 = new DiffValue("Test1");
                string arg = String.Empty;

                var dv2 = dv1.Add(value: arg);

                Assert.Same(expected: dv1, actual: dv2);
            }
        }

        public class AddMultiple
        {
            [Fact]
            public void ThrowsWhenGivenNullArg()
            {
                var dv = new DiffValue("Test1");
                IEnumerable<string> args = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => dv.Add(values: args));
                Assert.Equal(expected: "values", actual: ex.ParamName);
            }

            [Fact]
            public void ReturnsSameWhenGivenEmptyEnumerable()
            {
                var dv1 = new DiffValue("Test1");
                IEnumerable<string> args = Array.Empty<string>();

                var dv2 = dv1.Add(values: args);

                Assert.Same(expected: dv1, actual: dv2);
            }
        }

        public class RemoveSingle
        {
            [Fact]
            public void ThrowsWhenGivenNullArg()
            {
                var dv = new DiffValue("Test1");
                string arg = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => dv.Remove(value: arg));
                Assert.Equal(expected: "value", actual: ex.ParamName);
            }

            [Fact]
            public void ThrowsWhenSingleValue()
            {
                var dv = new DiffValue("Test");

                Assert.True(dv.IsSingleValue);
                var ex = Assert.Throws<InvalidOperationException>(() => dv.Remove("Test"));
                Assert.Equal(expected: "Cannot remove from a single-value DiffValue instance.", actual: ex.Message);
            }
        }

        public class RemoveMultiple
        {
            [Fact]
            public void ThrowsWhenGivenNullArg()
            {
                var dv = new DiffValue("Test1");
                IEnumerable<string> args = null!;

                var ex = Assert.Throws<ArgumentNullException>(() => dv.Remove(values: args));
                Assert.Equal(expected: "values", actual: ex.ParamName);
            }

            [Fact]
            public void ThrowsWhenSingleValue()
            {
                var dv = new DiffValue("Test");

                Assert.True(dv.IsSingleValue);
                var ex = Assert.Throws<InvalidOperationException>(() => dv.Remove(new[] { "Test" }));
                Assert.Equal(expected: "Cannot remove from a single-value DiffValue instance.", actual: ex.Message);
            }
        }

        public class GetDiffState
        {
            [Fact]
            public void TwoNullsYieldsNonExistent()
            {
                var state = DiffValue.GetDiffState(null, null);
                Assert.Equal(expected: DiffState.NonExistent, actual: state);
            }

            [Fact]
            public void NullOldValueYieldsNew()
            {
                var state = DiffValue.GetDiffState(oldValue: null, newValue: new DiffValue("Test"));
                Assert.Equal(expected: DiffState.New, actual: state);
            }

            [Fact]
            public void NullNewValueYieldsRemoved()
            {
                var state = DiffValue.GetDiffState(oldValue: new DiffValue("Test"), newValue: null);
                Assert.Equal(expected: DiffState.Removed, actual: state);
            }
        }

        public class GetEnumerator
        {
            [Fact]
            public void GetEnumeratorOnSingleValueIteratesOnlyOnce()
            {
                var dv = new DiffValue("Test");

                Assert.True(dv.IsSingleValue);

                int counter = 0;
                foreach (var _ in dv)
                {
                    counter++;
                }

                Assert.Equal(expected: 1, actual: counter);
            }


            [Fact]
            public void GetEnumeratorOnMultiValueIteratesForAllItems()
            {
                var dv = new DiffValue(new[] { "Test1", "Test2", "Test3" });

                Assert.False(dv.IsSingleValue);

                int counter = 0;
                foreach (var _ in dv)
                {
                    counter++;
                }

                Assert.Equal(expected: 3, actual: counter);
            }
        }
    }
}
