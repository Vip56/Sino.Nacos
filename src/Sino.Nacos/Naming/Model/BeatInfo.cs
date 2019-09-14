using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class BeatInfo
    {
        public int Port { get; set; }

        public string Ip { get; set; }

        public double Weight { get; set; }

        public string ServiceName { get; set; }

        public string Cluster { get; set; }

        public Dictionary<string, string> MetaData { get; set; }

        public bool Scheduled { get; set; }

        public long PerId { get; set; }

        public bool Stopped { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
