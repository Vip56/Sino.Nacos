using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sino.Nacos.Naming.Model
{
    /// <summary>
    /// 实例
    /// </summary>
    /// <remarks>
    /// 通过Metadata可以设定关于实例的其他系统参数，具体参数可以参考
    /// <see cref="PreservedMetadataKeys"/>
    /// </remarks>
    public class Instance
    {
        /// <summary>
        /// 实例编号
        /// </summary>
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        /// <summary>
        /// 实例IP
        /// </summary>
        [JsonProperty("ip")]
        public string Ip { get; set; }

        /// <summary>
        /// 实例端口
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// 实例权重
        /// </summary>
        [JsonProperty("weight")]
        public double Weight { get; set; }

        /// <summary>
        /// 实例健康状态
        /// </summary>
        [JsonProperty("healthy")]
        public bool Healthy { get; set; } = true;

        /// <summary>
        /// 实例启用状态
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 实例是临时(决定是否需要进行心跳检测),默认开启
        /// </summary>
        [JsonProperty("ephemeral")]
        public bool Ephemeral { get; set; } = true;

        /// <summary>
        /// 集群名称
        /// </summary>
        [JsonProperty("clusterName")]
        public string ClusterName { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToInetAddr()
        {
            return Ip + ":" + Port;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Instance))
            {
                return false;
            }
            Instance host = obj as Instance;
            return StrEquals(ToString(), host.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private static bool StrEquals(string str1, string str2)
        {
            return str1 == null ? str2 == null : str1.Equals(str2);
        }

        /// <summary>
        /// 获取实例心跳间隔时间
        /// </summary>
        public long GetInstanceHeartBeatInterval()
        {
            return GetMetaDataByKeyWithDefault(PreservedMetadataKeys.HEART_BEAT_INTERVAL, Constants.DEFAULT_HEART_BEAT_INTERVAL);
        }

        /// <summary>
        /// 获取实例心跳超时时间
        /// </summary>
        public long GetInstanceHeartBeatTimeOut()
        {
            return GetMetaDataByKeyWithDefault(PreservedMetadataKeys.HEART_BEAT_TIMEOUT, Constants.DEFAULT_HEART_BEAT_TIMEOUT);
        }

        /// <summary>
        /// 获取移除超时时间
        /// </summary>
        public long GetIpDeleteTimeout()
        {
            return GetMetaDataByKeyWithDefault(PreservedMetadataKeys.IP_DELETE_TIMEOUT, Constants.DEFAULT_IP_DELETE_TIMEOUT);
        }

        private long GetMetaDataByKeyWithDefault(string key, long defaultValue)
        {
            if (Metadata ==null || Metadata.Count <= 0)
            {
                return defaultValue;
            }
            string value = null;
            Metadata.TryGetValue(key, out value);
            if (!string.IsNullOrEmpty(value))
            {
                return long.Parse(value);
            }
            return defaultValue;
        }
    }
}
