using Sino.Nacos.Naming.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Core
{
    public class HostReactor
    {
        private Dictionary<string, ServiceInfo> _serviceInfoMap;

        public Dictionary<string, ServiceInfo> GetServiceInfoMap()
        {
            return _serviceInfoMap;
        }
    }
}
