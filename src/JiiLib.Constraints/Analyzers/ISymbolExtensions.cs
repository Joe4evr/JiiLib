using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace JiiLib.Constraints
{
    internal static class ISymbolExtensions
    {

        /// <summary>
        /// Checks if a given symbol implements an interface member implicitly
        /// </summary>
        public static bool IsImplementationOfAnyImplicitInterfaceMember<TSymbol>(this ISymbol symbol, [NotNullWhen(returnValue: true)] out TSymbol? implementedMember)
            where TSymbol : ISymbol
        {
            if (symbol.ContainingType != null)
            {
                foreach (INamedTypeSymbol interfaceSymbol in symbol.ContainingType.AllInterfaces)
                {
                    foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<TSymbol>())
                    {
                        if (IsImplementationOfInterfaceMember(symbol, interfaceMember))
                        {
                            implementedMember = interfaceMember;
                            return true;
                        }
                    }
                }
            }

            implementedMember = default;
            return false;
        }

        private static bool IsImplementationOfInterfaceMember(this ISymbol symbol, [NotNullWhen(returnValue: true)] ISymbol? interfaceMember)
        {
            return interfaceMember != null &&
                   symbol.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(interfaceMember), SymbolEqualityComparer.Default);
        }

        /// <summary>
        /// Checks if a given symbol implements an interface member or overrides an implementation of an interface member.
        /// </summary>
        private static bool IsOverrideOrImplementationOfInterfaceMember(this ISymbol symbol, [NotNullWhen(returnValue: true)] ISymbol? interfaceMember)
        {
            if (interfaceMember == null)
            {
                return false;
            }

            if (symbol.IsImplementationOfInterfaceMember(interfaceMember))
            {
                return true;
            }

            return symbol.IsOverride &&
                symbol.GetOverriddenMember()?.IsOverrideOrImplementationOfInterfaceMember(interfaceMember) == true;
        }

        /// <summary>
        /// Gets the symbol overridden by the given <paramref name="symbol"/>.
        /// </summary>
        /// <remarks>Requires that <see cref="ISymbol.IsOverride"/> is true for the given <paramref name="symbol"/>.</remarks>
        private static ISymbol GetOverriddenMember(this ISymbol symbol)
        {
            Debug.Assert(symbol.IsOverride);

            return symbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.OverriddenMethod!,

                IPropertySymbol propertySymbol => propertySymbol.OverriddenProperty!,

                IEventSymbol eventSymbol => eventSymbol.OverriddenEvent!,

                _ => throw new NotImplementedException(),
            };
        }
    }
}
