using Newtonsoft.Json;

namespace Madamin.Unfollow
{
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