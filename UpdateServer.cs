using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    internal class UpdateData
    {
        [JsonProperty("available")] public bool Available { get; set; }

        [JsonProperty("version")] public int Version { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("button_label")] public string Label { get; set; }

        [JsonProperty("button_url")] public string Url { get; set; }
    }

    internal class ExceptionData
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("callstack")] public string CallStack { get; set; }
    }

    internal abstract class RequestBase
    {
        [JsonProperty("method")] public abstract string Method { get; }
    }

    internal class CheckUpdateRequest : RequestBase
    {
        public override string Method => "check_update";

        [JsonProperty("version")] public int Version { get; set; }
    }

    internal class BugReportRequest : RequestBase
    {
        public override string Method => "bug_report";

        [JsonProperty("exception")] public ExceptionData Exception { get; set; }
    }

    internal class Response<TResult>
    {
        [JsonProperty("status")] public string Status { get; set; }

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
        [JsonProperty("id")] public int Id { get; set; }
    }

    internal class UpdateServerApi : IDisposable
    {
        public const string UpdateServerHost = "https://unfollowapp.herokuapp.com/api";
        public const string UpdateServerUserAgent = "UnfollowApp/v0.5";

        public UpdateServerApi(string apiAddress, string userAgent)
        {
            _client = new HttpClient();
            _apiAddress = apiAddress;
            _userAgent = userAgent;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<Response<TResponse>> SendRequest<TRequest, TResponse>(TRequest args)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _apiAddress);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None
            };
            var requestContent = JsonConvert.SerializeObject(
                args,
                typeof(TRequest),
                jsonSerializerSettings);
            request.Headers.UserAgent.ParseAdd(_userAgent);
            request.Content = new StringContent(
                requestContent,
                Encoding.UTF8,
                "application/json");
            var result = await _client.SendAsync(request);
            if (result.Content.Headers.ContentType.MediaType != "application/json")
                throw new Exception("invalid result for MediaType");
            var response = await result.Content.ReadAsStringAsync();
            return (Response<TResponse>) JsonConvert.DeserializeObject(response, typeof(Response<TResponse>));
        }

        public async Task<Response<CheckUpdateResult>> CheckUpdate(CheckUpdateRequest args)
        {
            return await SendRequest<CheckUpdateRequest, CheckUpdateResult>(args);
        }

        public async Task<Response<BugReportResult>> BugReport(BugReportRequest args)
        {
            return await SendRequest<BugReportRequest, BugReportResult>(args);
        }

        private readonly string _apiAddress;
        private readonly string _userAgent;
        private readonly HttpClient _client;
    }
}