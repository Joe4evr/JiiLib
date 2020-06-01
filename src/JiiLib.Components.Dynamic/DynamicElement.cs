using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using JiiLib.Components.Dynamic.Extensions;
using JiiLib.Components.Dynamic.Rendering;

namespace JiiLib.Components.Dynamic
{
    public sealed partial class DynamicElement : ComponentBase
    {
#nullable disable warnings
        [Inject] public IRendererFactory Factory { private get; set; }
#nullable enable

        [Parameter] public string? Contents { private get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Contents is null)
                return;

            var root = new Node(null, "Hello world!");
            var facade = new BuilderFacade(builder, Factory);
            var renderer = Factory.GetRendererForTag(root.Tag);
            renderer.BuildRenderTree(facade, root);
        }
    }
}
