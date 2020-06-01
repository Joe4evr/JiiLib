using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using JiiLib.Components.Dynamic.Extensions;
using JiiLib.Components.Dynamic.Rendering;
using JiiLib.Components.Dynamic.Rendering.Renderers;

namespace JiiLib.Components.Dynamic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicRendererFactory(
            this IServiceCollection services,
            Action<IRendererFactoryBuilder> builderAction)
        {
            var builder = new DefaultFactoryBuilder();
            builderAction?.Invoke(builder);
            builder.AddFactory(services);

            return services;
        }

        private sealed class DefaultFactoryBuilder : IRendererFactoryBuilder
        {
            private readonly Dictionary<string, Type> _renderers;
            private readonly Type _nullTagRenderer = typeof(NullTagRenderer);

            public DefaultFactoryBuilder()
            {
                var btr = typeof(BasicTagRenderer);
                _renderers = new Dictionary<string, Type>
                {
                    ["strong"] = btr,
                    ["span"] = btr,
                };
            }


            IRendererFactoryBuilder IRendererFactoryBuilder.AddRenderer<TRenderer>(string tag)
            {
                _renderers[tag] = typeof(TRenderer);
                return this;
            }

            internal void AddFactory(IServiceCollection services)
            {
                services.AddSingleton<IRendererFactory>(
                    svcs => new DefaultFactory(_renderers, _nullTagRenderer, svcs));
            }
        }
    }
}
