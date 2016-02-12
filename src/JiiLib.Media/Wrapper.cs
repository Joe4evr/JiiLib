using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiiLib.Media
{
    internal class Wrapper<T>
    {
        internal T Value { get; }

        internal Wrapper(T item)
        {
            Value = item;
        }
    }
}
