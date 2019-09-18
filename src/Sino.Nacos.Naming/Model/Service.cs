using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class Service
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("protectThreshold")]
        public float ProtectThreshold { get; set; }

        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public Service() { }

        public Service(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
