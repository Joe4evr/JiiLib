using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Represents a contract for necessary lookups.
    /// </summary>
    /// <typeparam name="T">
    ///     The element type used for <see cref="QueryInterpreter{T}"/>.
    /// </typeparam>
    public interface IInterpreterConfig<out T>
    {
        /// <summary>
        ///     If supported, formats a string to be displayed a certain way.
        /// </summary>
        /// <param name="value">
        ///     The input string.
        /// </param>
        /// <param name="formats">
        ///     The desired formats.
        /// </param>
        /// <returns>
        ///     A string that will be displayed in the specified format on the target environment.
        /// </returns>
        string FormatString(string value, FormatModifiers formats);

        /// <summary>
        ///     Gets a method that will perform a 'Contains' operation on a given type.
        /// </summary>
        /// <param name="elementType">
        ///     The type of the element.
        /// </param>
        /// <remarks>
        ///     <note type="implementer">
        ///         This must reference a static method that takes in an <see cref="IEnumerable{T}"/>
        ///         of the <paramref name="elementType"/> and a <see cref="String"/> and returns a <see cref="Boolean"/>.
        ///     </note>
        /// </remarks>
        MethodInfo GetContainsMethod(Type elementType);

        /// <summary>
        ///     Gets a method that will perform an 'OrderBy' operation on a given type.
        /// </summary>
        /// <param name="elementType">
        ///     The type of the element.
        /// </param>
        /// <param name="descending">
        ///     Indicates if the ordering should be done ascending or descending.
        /// </param>
        /// <remarks>
        ///     <note type="implementer">
        ///         This must reference a static method that takes in an <see cref="IEnumerable{T}"/>
        ///         of the <paramref name="elementType"/>, a <see cref="Func{T, TResult}"/>
        ///         of the <paramref name="elementType"/> to <see cref="Int32"/>,
        ///         and returns an <see cref="IOrderedEnumerable{TElement}"/> of the <paramref name="elementType"/>.
        ///     </note>
        /// </remarks>
        MethodInfo GetOrderByMethod(Type elementType, bool descending);

        /// <summary>
        ///     Gets a method that will perform a 'ThenBy' operation on a given type.
        /// </summary>
        /// <param name="elementType">
        ///     The type of the element.
        /// </param>
        /// <param name="descending">
        ///     Indicates if the ordering should be done ascending or descending.
        /// </param>
        /// <remarks>
        ///     <note type="implementer">
        ///         This must reference a static method that takes in an <see cref="IOrderedEnumerable{TElement}"/>
        ///         of the <paramref name="elementType"/>, a <see cref="Func{T, TResult}"/>
        ///         of the <paramref name="elementType"/> to <see cref="Int32"/>,
        ///         and returns an <see cref="IOrderedEnumerable{TElement}"/> of the <paramref name="elementType"/>.
        ///     </note>
        /// </remarks>
        MethodInfo GetThenByMethod(Type elementType, bool descending);
    }
}
