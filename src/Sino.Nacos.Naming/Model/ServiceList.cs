using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class ServiceList
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("doms")]
        public List<string> Doms { get; set; } = new List<string>();
    }
}
