using Newtonsoft.Json;

namespace api
{
    public class Counter
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }
    }
}
