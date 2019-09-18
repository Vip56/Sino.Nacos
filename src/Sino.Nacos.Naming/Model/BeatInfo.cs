using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    /// <summary>
    /// 心跳参数
    /// </summary>
    public class BeatInfo
    {
        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("cluster")]
        public string Cluster { get; set; }

        [JsonProperty("metaData")]
        public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();

        [JsonProperty("scheduled")]
        public bool Scheduled { get; set; }

        [JsonProperty("perId")]
        public long PerId { get; set; }

        [JsonProperty("stopped")]
        public bool Stopped { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
