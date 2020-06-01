using System;

namespace JiiLib.Components.Dynamic.Rendering
{
    public interface IRenderer
    {
        RenderContinuation BuildRenderTree(IRenderBuilderFacade builder, Node node);
    }
}
