using Newtonsoft.Json;

namespace Sino.Nacos.Naming.Model
{
    /// <summary>
    /// Nacos服务度量指标
    /// </summary>
    public class ServiceMetrics
    {
        [JsonProperty("serviceCount")]
        public int ServiceCount { get; set; }

        [JsonProperty("load")]
        public float Load { get; set; }

        [JsonProperty("mem")]
        public float Mem { get; set; }

        [JsonProperty("responsibleServiceCount")]
        public int ResponsibleServiceCount { get; set; }

        [JsonProperty("instanceCount")]
        public int InstanceCount { get; set; }

        [JsonProperty("cpu")]
        public float Cpu { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("responsibleInstanceCount")]
        public int ResponsibleInstanceCount { get; set; }
    }
}
