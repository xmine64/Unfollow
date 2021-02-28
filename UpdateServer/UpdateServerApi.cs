using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    internal partial class UpdateServerApi : IDisposable
    {
        private readonly HttpClient _client;

        public UpdateServerApi()
        {
            _client = new HttpClient();
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
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiAddress);
            httpRequest.Headers.UserAgent.ParseAdd(UserAgent);

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
                throw new UnexpectedResult();
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

#if TGBUILD || DEBUG
        public async Task<ApiResponse> DidLogin(long version)
        {
            return await SendRequest(new ApiRequest
            {
                Method = MethodDidLogin,
                Version = version
            });
        }
#endif
    }

    public class UnexpectedResult : Exception
    {
    }
}
