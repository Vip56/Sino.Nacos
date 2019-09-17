using Sino.Nacos.Naming.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Listener
{
    /// <summary>
    /// 服务事件
    /// </summary>
    public class NamingEvent : IEvent
    {
        public string ServiceName { get; set; }

        public string GroupName { get; set; }

        public string Clusters { get; set; }

        public IReadOnlyCollection<Instance> Instances { get; set; }

        public NamingEvent(string serviceName, IReadOnlyCollection<Instance> instances)
        {
            this.ServiceName = serviceName;
            this.Instances = instances;
        }

        public NamingEvent(string serviceName, string groupName, string clusters, IReadOnlyCollection<Instance> instances)
        {
            this.ServiceName = serviceName;
            this.GroupName = groupName;
            this.Clusters = clusters;
            this.Instances = instances;
        }
    }
}
