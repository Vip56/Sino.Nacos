using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class Service
    {
        public string Name { get; set; }

        public float ProtectThreshold { get; set; }

        public string AppName { get; set; }

        public string GroupName { get; set; }

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public Service() { }

        public Service(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return $"Service{{name='{Name}', protectThreshold={ProtectThreshold}, appName='{AppName}', groupName='{GroupName}', metadata='{Metadata}'}}";
        }
    }
}
