using System;

namespace JiiLib.Components.Dynamic.Rendering
{
    public interface IRendererFactory
    {
        IRenderer GetRendererForTag(string? tag);
    }
}
