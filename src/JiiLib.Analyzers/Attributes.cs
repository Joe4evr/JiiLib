namespace JiiLib.Analyzers;

[AttributeUsage(AttributeTargets.Method)]
public sealed class NoDefensiveCopiesAttribute : Attribute
{
#pragma warning disable IDE0060 // Remove unused parameter
    public NoDefensiveCopiesAttribute(params string[] variables) { }
#pragma warning restore IDE0060 // Remove unused parameter
}
