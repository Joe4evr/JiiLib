using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace JiiLib.Components.Dynamic
{
    public interface IRenderBuilderFacade
    {
        void AddAttribute(string name, string? value);
        void AddRenderFragment(string fragmentName, IReadOnlyCollection<Node> contentNodes);
        void AddContent(string textContent);
        void CloseComponent();
        void CloseElement();
        void OpenComponent<TComponent>() where TComponent : IComponent;
        void OpenElement(string elementName);
    }
}
