namespace JiiLib.Analyzers;

/// <summary>
///     Indicates this method is analyzed to prevent
///     some or all struct variables from getting
///     defensively copied.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class NoDefensiveCopiesAttribute : Attribute
{
    /// <summary/>
    /// <param name="variables">
    ///     The names of variables that are <em>not</em> allowed to
    ///     be defensively copied. These can be any properties,
    ///     fields, locals, or parameters used in the method.
    ///     Leave empty to disallow <em>any</em> applicable
    ///     variables be defensively copied.
    /// </param>
#pragma warning disable IDE0060 // Remove unused parameter
    public NoDefensiveCopiesAttribute(params string[] variables) { }
#pragma warning restore IDE0060 // Remove unused parameter
}
