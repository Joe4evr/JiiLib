using System;
using System.Threading.Tasks;
using JiiLib.Collections.DiffList;

namespace JiiLib.Components
{
    /// <summary>
    ///     Defines a contract for storing and retrieving
    ///     instances of <see cref="KeyedDiffList{TKey}"/>.
    /// </summary>
    public interface IDiffListRepository
    {
        /// <summary>
        ///     Stores a <see cref="KeyedDiffList{TKey}"/> in the repository.
        /// </summary>
        /// <param name="list">
        ///     The list to store.
        /// </param>
        /// <returns>
        ///     An ID that allows retrieving a copy of the list at a later time.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="list"/> was <see langword="null"/>.
        /// </exception>
        Task<string> AddListAsync(KeyedDiffList<string> list);

        /// <summary>
        ///     Retrieves a <see cref="KeyedDiffList{TKey}"/> from the repository.
        /// </summary>
        /// <param name="listId">
        ///     The ID of the list to retrieve.
        /// </param>
        /// <returns>
        ///     A reconstructed instance of a <see cref="KeyedDiffList{TKey}"/>
        ///     as it was stored, or <see langword="null"/> if the
        ///     <paramref name="listId"/> was not found.
        /// </returns>
        Task<KeyedDiffList<string>?> GetListAsync(string listId);

        /// <summary>
        ///     Gets the amount of lists stored in the repository.
        /// </summary>
        /// <returns>
        ///     The amount of lists stored.
        /// </returns>
        Task<int> GetCountAsync();
    }
}
