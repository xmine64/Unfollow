using Newtonsoft.Json;

namespace Madamin.Unfollow
{
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
}