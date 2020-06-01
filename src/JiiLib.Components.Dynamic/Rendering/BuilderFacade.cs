using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace JiiLib.Components.Dynamic.Rendering
{
    internal sealed class BuilderFacade : IRenderBuilderFacade
    {
        private readonly RenderTreeBuilder _builder;
        private readonly IRendererFactory _factory;

        private int _counter = 0;

        public BuilderFacade(RenderTreeBuilder builder, IRendererFactory factory)
        {
            _builder = builder;
            _factory = factory;
        }

        public void AddAttribute(string name, string? value)
            => _builder.AddAttribute(Interlocked.Increment(ref _counter), name, value);
        public void AddRenderFragment(string fragmentName, IReadOnlyCollection<Node> contentNodes)
        {
            if (contentNodes.Count > 0)
            {
                _builder.AddAttribute(Interlocked.Increment(ref _counter), fragmentName,
                    (RenderFragment)(b2 =>
                    {
                        var facadeCopy = new BuilderFacade(b2, _factory) { _counter = _counter };
                        foreach (var node in contentNodes)
                        {
                            var renderer = _factory.GetRendererForTag(node.Tag);
                            renderer.BuildRenderTree(facadeCopy, node);
                        }

                        _counter = facadeCopy._counter;
                    }));
            }
        }
        public void AddContent(string textContent)
            => _builder.AddMarkupContent(Interlocked.Increment(ref _counter), textContent);
        public void CloseComponent() => _builder.CloseComponent();
        public void CloseElement() => _builder.CloseElement();
        public void OpenComponent<TComponent>()
            where TComponent : IComponent
            => _builder.OpenComponent<TComponent>(Interlocked.Increment(ref _counter));
        public void OpenElement(string elementName)
            => _builder.OpenElement(Interlocked.Increment(ref _counter), elementName);
    }
}
