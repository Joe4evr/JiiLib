using System;

namespace JiiLib.Constraints
{
    /// <summary>
    ///     Indicates that this type parameter accepts only interface types.
    /// </summary>
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
    public sealed class InterfacesOnlyAttribute : Attribute { }

    /// <summary>
    ///     Indicates that this type parameter accepts only non-abstract types.
    /// </summary>
    /// <remarks>
    ///     This is like the 'new()' constraint but without
    ///     the requirement of a public parameter-less constructor.
    /// </remarks>
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
    public sealed class NonAbstractOnlyAttribute : Attribute { }

    /// <summary>
    ///     Indicates that this type parameter accepts only the type
    ///     that implements/extends/inherits this generic interface/class.
    /// </summary>
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
    public sealed class SelfTypeAttribute : Attribute { }
}
