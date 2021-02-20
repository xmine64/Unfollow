using Newtonsoft.Json;

namespace Madamin.Unfollow
{
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
}