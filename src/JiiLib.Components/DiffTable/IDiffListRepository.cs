using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JiiLib.Collections.DiffList;

namespace JiiLib.Components
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDiffListRepository
    {
        //Task<KeyedDiffList<string>?> GetListAsync(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list">
        /// </param>
        /// <returns>
        /// </returns>
        Task<string> AddListAsync(KeyedDiffList<string> list);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">
        /// 
        /// </param>
        /// <param name="comparer">
        /// 
        /// </param>
        /// <param name="comparison">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        Task<KeyedDiffList<string>?> GetListAsync(string listId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// </returns>
        Task<int> GetCountAsync();
    }
}
