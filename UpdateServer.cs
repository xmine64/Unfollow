using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    class UpdateData
    {
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

    class ResponseBase
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    class CheckUpdateResponse : ResponseBase
    {
        [JsonProperty("update", Required = Required.DisallowNull)]
        public UpdateData Update { get; set; }
    }

    class BugReportResponse : ResponseBase
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

        private async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest args)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _api_address);
            request.Headers.UserAgent.ParseAdd(_user_agent);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(
                    args,
                    typeof(TRequest),
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.None
                    }
                    ),
                Encoding.UTF8,
                "application/json");
            var result = await _client.SendAsync(request);
            var response = await result.Content.ReadAsStringAsync();
            return (TResponse)JsonConvert.DeserializeObject(response, typeof(TResponse));
        }

        public async Task<CheckUpdateResponse> CheckUpdate(CheckUpdateRequest args)
        {
            return await SendRequest<CheckUpdateRequest, CheckUpdateResponse>(args);
        }

        public async Task<BugReportResponse> BugReport(BugReportRequest args)
        {
            return await SendRequest<BugReportRequest, BugReportResponse>(args);
        }

        private string _api_address, _user_agent;
        private HttpClient _client;
    }
}
