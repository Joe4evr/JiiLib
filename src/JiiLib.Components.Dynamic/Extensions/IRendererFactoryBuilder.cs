using System;
using JiiLib.Components.Dynamic.Rendering;

namespace JiiLib.Components.Dynamic.Extensions
{
    public interface IRendererFactoryBuilder
    {
        IRendererFactoryBuilder AddRenderer<TRenderer>(string tag)
            where TRenderer : IRenderer;
    }
}
