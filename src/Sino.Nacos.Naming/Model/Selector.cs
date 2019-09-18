using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class Selector
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
