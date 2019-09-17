using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    /// <summary>
    /// Nacos服务度量指标
    /// </summary>
    public class ServiceMetrics
    {
        public int ServiceCount { get; set; }

        public float Load { get; set; }

        public float Mem { get; set; }

        public int ResponsibleServiceCount { get; set; }

        public int InstanceCount { get; set; }

        public float Cpu { get; set; }

        public string Status { get; set; }

        public int ResponsibleInstanceCount { get; set; }
    }
}
