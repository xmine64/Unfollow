using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    class UpdateData
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

    class ExceptionData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("callstack")]
        public string Callstack { get; set; }
    }

    abstract class RequestBase
    {
        [JsonProperty("method")]
        public abstract string Method { get; }
    }

    class CheckUpdateRequest : RequestBase
    {
        public override string Method => "check_update";

        [JsonProperty("version")]
        public int Version { get; set; }

    }

    class BugReportRequest : RequestBase
    {
        public override string Method => "bug_report";

        [JsonProperty("exception")]
        public ExceptionData Exception { get; set; }
    }

    class Response<TResult>
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result", Required = Required.DisallowNull)]
        public TResult Result { get; set; }
    }

    class CheckUpdateResult
    {
        [JsonProperty("update", Required = Required.DisallowNull)]
        public UpdateData Update { get; set; }
    }

    class BugReportResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    class UpdateServerApi : IDisposable
    {
        public UpdateServerApi(string api_address, string user_agent)
        {
            _client = new HttpClient();
            _api_address = api_address;
            _user_agent = user_agent;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<Response<TResponse>> SendRequest<TRequest, TResponse>(TRequest args)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _api_address);
            var request_content = JsonConvert.SerializeObject(
                    args,
                    typeof(TRequest),
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.None
                    }
                    );
            request.Headers.UserAgent.ParseAdd(_user_agent);
            request.Content = new StringContent(
                request_content,
                Encoding.UTF8,
                "application/json");
            var result = await _client.SendAsync(request);
            if (result.Content.Headers.ContentType.MediaType != "application/json")
                throw new Exception("invalid result for MediaType");
            var response = await result.Content.ReadAsStringAsync();
            return (Response<TResponse>)JsonConvert.DeserializeObject(response, typeof(Response<TResponse>));
        }

        public async Task<Response<CheckUpdateResult>> CheckUpdate(CheckUpdateRequest args)
        {
            return await SendRequest<CheckUpdateRequest, CheckUpdateResult>(args);
        }

        public async Task<Response<BugReportResult>> BugReport(BugReportRequest args)
        {
            return await SendRequest<BugReportRequest, BugReportResult>(args);
        }

        private string _api_address, _user_agent;
        private HttpClient _client;
    }
}
