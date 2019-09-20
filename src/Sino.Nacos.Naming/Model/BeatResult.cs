using Newtonsoft.Json;

namespace Sino.Nacos.Naming.Model
{
    public class BeatResult
    {
        [JsonProperty("clientBeatInterval")]
        public int ClientBeatInterval { get; set; }
    }
}
