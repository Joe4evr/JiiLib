#if !DOTNET5_4
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;

namespace JiiLib.Net
{
    /// <summary>
    /// To be used in conjunction with a Google Script that returns data in a JSON format.
    /// </summary>
    /// <remarks>Information on how to make a Google Script can be found here: https://developers.google.com/apps-script/guides/rest/quickstart/target-script </remarks>
    public class GoogleScriptApiService : IJsonApiService
    {
        private readonly string _projectKey;
        private readonly string _appName;
        private readonly string _funcName;
        private readonly string[] _scopes;
        private readonly UserCredential _credential;

        /// <summary>
        /// List of parameters to pass into the Google Script function (optional).
        /// </summary>
        public IList<object> Parameters { get; set; }

        /// <summary>
        /// Create a new instance of a GoogleScriptApiService
        /// </summary>
        /// <param name="secretsFile">Path to the client_secret.json file downloaded from the Google developer console.</param>
        /// <param name="credStorePath">Path to the directory where you wish to store the credentials.</param>
        /// <param name="applicationName">Name of your application.</param>
        /// <param name="projectKey">Project key of the script you wish to invoke.</param>
        /// <param name="functionName">Name of the script function you wish to invoke.</param>
        /// <param name="neededScopes">The set of Scopes required for your application.</param>
        /// <exception cref="ArgumentNullException">Any parameter was null.</exception>
        /// <exception cref="FileNotFoundException">Parameter <see cref="secretsFile" /> was not found or not a file.</exception>
        /// <exception cref="DirectoryNotFoundException">Parameter <see cref="credStorePath" /> was not found or not a directory.</exception>
        public GoogleScriptApiService(
            string secretsFile,
            string credStorePath,
            string applicationName,
            string projectKey,
            string functionName,
            string[] neededScopes)
        {
            if (secretsFile == null) throw new ArgumentNullException(nameof(secretsFile));
            if (credStorePath == null) throw new ArgumentNullException(nameof(credStorePath));

            if (!File.Exists(secretsFile)) throw new FileNotFoundException(nameof(secretsFile));
            if (!Directory.Exists(credStorePath)) throw new DirectoryNotFoundException(nameof(credStorePath));

            if (projectKey == null) throw new ArgumentNullException(nameof(projectKey));
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));
            if (functionName == null) throw new ArgumentNullException(nameof(functionName));
            if (neededScopes == null) throw new ArgumentNullException(nameof(neededScopes));
            
            _projectKey = projectKey;
            _appName = applicationName;
            _funcName = functionName;
            _scopes = neededScopes;

            using (Stream sr = new FileStream(secretsFile, FileMode.Open, FileAccess.Read))
            {
                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(sr).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credStorePath, fullPath: true)).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Preparing to call the API service.
        /// </summary>
        /// <returns>The Operation representing the API call</returns>
        private async Task<Operation> ExecuteOperaionAsync()
        {
            var service = new ScriptService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                ApplicationName = _appName
            });

            var request = new ExecutionRequest
            {
                Function = _funcName,
                //DevMode = Debugger.IsAttached,
                Parameters = this.Parameters
            };

            ScriptsResource.RunRequest runReq = service.Scripts.Run(request, _projectKey);

            return await runReq.ExecuteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the requested data from the API formatted as JSON.
        /// </summary>
        /// <returns>A JSON-formatted string representing the requested data.</returns>
        public async Task<string> GetDataFromServiceAsJsonAsync()
        {
            try
            {
                // Make the API request.
                Operation op = await ExecuteOperaionAsync().ConfigureAwait(false);

                if (op.Error == null) //success
                {
                    return (string)op.Response["result"];
                }
                else
                {
                    // The API executed, but the script returned an error.
                    throw new AggregateException("The operation excuted, but the scirpt returned an error.");

                    // Extract the first (and only) set of error details
                    // as a IDictionary. The values of this dictionary are
                    // the script's 'errorMessage' and 'errorType', and an
                    // array of stack trace elements. Casting the array as
                    // a JSON JArray allows the trace elements to be accessed
                    // directly.

                    //IDictionary<string, object> error = op.Error.Details[0];
                    //Console.WriteLine($"Script error message: {error["errorMessage"]}");
                    //if (error["scriptStackTraceElements"] != null)
                    //{
                    //    // There may not be a stacktrace if the script didn't
                    //    // start executing.
                    //    Console.WriteLine("Script error stacktrace:");
                    //    JArray st = (JArray)error["scriptStackTraceElements"];
                    //    foreach (var trace in st)
                    //    {
                    //        Console.WriteLine($"\t{trace["function"]}: {trace["lineNumber"]}");
                    //    }
                    //}
                }
            }
            catch (Google.GoogleApiException e)
            {
                throw new AggregateException("The operation threw an exception, see the InnerException for details.", e);
            }
        }
    }
}
#endif
