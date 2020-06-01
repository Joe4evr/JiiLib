using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JiiLib.Components.Dynamic.Rendering
{
    internal sealed class DefaultFactory : IRendererFactory
    {
        private readonly ImmutableDictionary<string, Type> _renderTypes;
        private readonly Type _nullRenderer;
        private readonly IServiceProvider _services;

        internal DefaultFactory(Dictionary<string, Type> renderTypes,
            Type nullRenderer, IServiceProvider services)
        {
            _renderTypes = ImmutableDictionary.CreateRange(renderTypes);
            _nullRenderer = nullRenderer;
            _services = services;
        }

        public IRenderer GetRendererForTag(string? tag)
        {
            //if (tag is null)


            throw new NotImplementedException();
        }
    }
}
