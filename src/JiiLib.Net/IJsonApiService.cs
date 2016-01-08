using System.Threading.Tasks;

namespace JiiLib.Net
{
    /// <summary>
    /// Contract for a service that can return data as JSON.
    /// </summary>
    public interface IJsonApiService
    {
        /// <summary>
        /// Gets the requested data from the API formatted as JSON.
        /// </summary>
        /// <returns>A JSON-formatted string representing the requested data.</returns>
        Task<string> GetDataFromServiceAsJsonAsync();
    }
}