﻿using System;
using System.Collections.Generic;
using System.Text;

namespace JiiLib.Components.Dynamic.Rendering.Renderers
{
    internal sealed class NullTagRenderer : IRenderer
    {
        public RenderContinuation BuildRenderTree(IRenderBuilderFacade builder, Node node)
        {
            throw new NotImplementedException();
        }
    }
}
