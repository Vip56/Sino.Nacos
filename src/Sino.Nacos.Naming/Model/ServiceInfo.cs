using Newtonsoft.Json;
using Sino.Nacos.Naming.Utils;
using System;
using System.Collections.Generic;
using System.Web;

namespace Sino.Nacos.Naming.Model
{
    /// <summary>
    /// 服务信息
    /// </summary>
    public class ServiceInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("clusters")]
        public string Clusters { get; set; }

        [JsonProperty("cacheMillis")]
        public long CacheMillis { get; set; } = 1000;

        [JsonProperty("hosts")]
        public IList<Instance> Hosts { get; set; } = new List<Instance>();

        [JsonProperty("lastRefTime")]
        public long LastRefTime { get; set; } = 0;

        [JsonProperty("checksum")]
        public string CheckSum { get; set; } = "";

        [JsonProperty("allips")]
        public bool AllIPs { get; set; } = false;

        public ServiceInfo()
        {

        }

        public ServiceInfo(string name, string clusters)
        {
            this.Name = name;
            this.Clusters = clusters;
        }

        public int IpCount()
        {
            return Hosts.Count;
        }

        public bool Expired()
        {
            return DateTime.Now.GetTimeStamp() - LastRefTime > CacheMillis;
        }

        public bool IsValid()
        {
            return Hosts != null;
        }

        public bool Validate()
        {
            if (AllIPs)
            {
                return true;
            }

            IList<Instance> validHosts = new List<Instance>();
            foreach(var host in Hosts)
            {
                if (!host.Healthy)
                {
                    continue;
                }
                for(int i = 0; i < host.Weight; i++)
                {
                    validHosts.Add(host);
                }
            }
            return true;
        }

        public string GetKey()
        {
            return GetKey(Name, Clusters);
        }

        public string GetKeyEncoded()
        {
            return GetKey(HttpUtility.UrlEncode(Name), Clusters);
        }

        public static string GetKey(string name, string clusters)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name + Constants.SERVICE_INFO_SPLITER + clusters;
            }
            return name;
        }

        public static ServiceInfo FromKey(string key)
        {
            ServiceInfo serviceInfo = new ServiceInfo();
            int maxSegCount = 3;
            string[] segs = key.Split(new string[] { Constants.SERVICE_INFO_SPLITER }, StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length == maxSegCount - 1)
            {
                serviceInfo.GroupName = segs[0];
                serviceInfo.Name = segs[1];
            }
            else if(segs.Length == maxSegCount)
            {
                serviceInfo.GroupName = segs[0];
                serviceInfo.Name = segs[1];
                serviceInfo.Clusters = segs[2];
            }
            return serviceInfo;
        }

        public override string ToString()
        {
            return GetKey();
        }
    }
}
