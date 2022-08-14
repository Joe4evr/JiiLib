using System;

namespace System.Diagnostics.CodeAnalysis
{
    internal sealed class NotNullWhenAttribute : Attribute
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public NotNullWhenAttribute(bool returnValue) { }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
