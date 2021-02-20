using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Newtonsoft.Json;

namespace Madamin.Unfollow
{
    internal partial class UpdateServerApi : IDisposable
    {
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
}