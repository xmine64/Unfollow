using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    internal class UpdateServerApi : IDisposable
    {
        public const string StatusOk = "ok";

        private const string MethodCheckUpdate = "check_update";
        private const string MethodBugReport = "bug_report";
#if TGBUILD
        private const string MethodDidLogin = "did_login";
#endif

#if TGBUILD
        public const string LanguageEnglish = "en";
        public const string LanguagePersian = "fa";
#else
        public const string LanguageGithubChannel = "github";
#endif

        private const string NotAvailable = "Not Available";

        private const string JsonMimeType = "application/json";

        private readonly string _apiAddress;
        private readonly HttpClient _client;
        private readonly Context _context;
        private readonly string _userAgent;

        public UpdateServerApi(Context context)
        {
            _context = context;
            _client = new HttpClient();
            _apiAddress = _context.GetString(Resource.String.url_update_api);
            _userAgent = _context.GetString(Resource.String.update_api_user_agent);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<ApiResponse> SendRequest(ApiRequest request)
        {
            // Serialize request
            var content = JsonConvert.SerializeObject(request);

            // Make a request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiAddress);
            httpRequest.Headers.UserAgent.ParseAdd(_userAgent);

            httpRequest.Content = new StringContent(
                content,
                Encoding.UTF8,
                JsonMimeType);

            // Send request
            var result = await _client.SendAsync(httpRequest);

            // Check response
            var responseContentType = result.Content.Headers.ContentType.MediaType;
            if (responseContentType != JsonMimeType)
            {
                throw new Exception(
                    _context.GetString(
                        Resource.String.msg_invalid_mime,
                        result.Content.Headers.ContentType.MediaType));
            }

            // Return response
            var response = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(response);
        }

        public async Task<ApiResponse> CheckUpdate(long version, string language)
        {
            return await SendRequest(new ApiRequest
            {
                Method = MethodCheckUpdate,
                Version = version,
                Language = language
            });
        }

        public async Task<ApiResponse> BugReport(Exception exception)
        {
            if (exception.InnerException != null)
            {
                await BugReport(exception);
            }
            return await SendRequest(new ApiRequest
            {
                Method = MethodBugReport,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                Callstack = string.IsNullOrWhiteSpace(exception.StackTrace) ? NotAvailable : exception.StackTrace
            });
        }

#if TGBUILD
        public async Task<ApiResponse> DidLogin(int version)
        {
            return await SendRequest(new ApiRequest
            {
                Method = MethodDidLogin,
                Version = version
            });
        }
#endif
    }

    internal class ApiRequest
    {
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }
        
        [JsonProperty("version", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Version { get; set; }

        [JsonProperty("lang", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }

        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionType { get; set; }
        
        [JsonProperty("message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("callstack", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Callstack { get; set; }

    }

    internal class ApiResponse
    {
        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("error", Required = Required.DisallowNull)]
        public string Error { get; set; }

        [JsonProperty("message", Required = Required.DisallowNull)]
        public string Message { get; set; }

        [JsonProperty("available", Required = Required.DisallowNull)]
        public bool Available { get; set; }

        [JsonProperty("update", Required = Required.DisallowNull)]
        public UpdateInformation Update { get; set; }
    }

    internal class UpdateInformation
    {
        [JsonProperty("message", Required = Required.DisallowNull)]
        public string Message { get; set; }

        [JsonProperty("version", Required = Required.DisallowNull)]
        public long Version { get; set; }

        [JsonProperty("button_label", Required = Required.DisallowNull)]
        public string ButtonLabel { get; set; }

        [JsonProperty("button_url", Required = Required.DisallowNull)]
        public string ButtonUrl { get; set; }
    }
}