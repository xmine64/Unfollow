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
        public const string ButtonOk = "unfollow:ok";

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

        private async Task<string> SendRequest(string content)
        {
            // Make a request
            var request = new HttpRequestMessage(HttpMethod.Post, _apiAddress);
            request.Headers.UserAgent.ParseAdd(_userAgent);

            request.Content = new StringContent(
                content,
                Encoding.UTF8,
                JsonMimeType);

            // Send request
            var result = await _client.SendAsync(request);

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
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<Response<CheckUpdateResult>> CheckUpdate(CheckUpdateRequest args)
        {
            var request = JsonConvert.SerializeObject(args);
            var response = await SendRequest(request);
            return JsonConvert.DeserializeObject<Response<CheckUpdateResult>>(response);
        }

        public async Task<Response<BugReportResult>> BugReport(BugReportRequest args)
        {
            var request = JsonConvert.SerializeObject(args);
            var response = await SendRequest(request);
            return JsonConvert.DeserializeObject<Response<BugReportResult>>(response);
        }
    }

    internal class UpdateData
    {
        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("button_label")]
        public string Label { get; set; }

        [JsonProperty("button_url")]
        public string Url { get; set; }
    }

    internal class ExceptionData
    {
        [JsonProperty("type")] 
        public string Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("callstack")]
        public string CallStack { get; set; }
    }

    internal class CheckUpdateRequest
    {
        [JsonProperty("method")]
        public string Method { get; } = "check_update";

        [JsonProperty("version")] 
        public int Version { get; set; }
    }

    internal class BugReportRequest
    {
        [JsonProperty("method")]
        public string Method { get; } = "bug_report";

        [JsonProperty("exception")]
        public ExceptionData Exception { get; set; }
    }

    internal class Response<TResult>
    {
        [JsonProperty("status")] 
        public string Status { get; set; }

        [JsonProperty("result", Required = Required.DisallowNull)]
        public TResult Result { get; set; }

        [JsonProperty("error", Required = Required.DisallowNull)]
        public string Error { get; set; }
    }

    internal class CheckUpdateResult
    {
        [JsonProperty("update", Required = Required.DisallowNull)]
        public UpdateData Update { get; set; }
    }

    internal class BugReportResult
    {
        [JsonProperty("id")] 
        public int Id { get; set; }
    }
}