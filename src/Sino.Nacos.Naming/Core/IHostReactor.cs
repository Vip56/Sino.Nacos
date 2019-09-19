using Sino.Nacos.Naming.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Core
{
    public interface IHostReactor
    {
        Dictionary<string, ServiceInfo> GetServiceInfoMap();
    }
}
