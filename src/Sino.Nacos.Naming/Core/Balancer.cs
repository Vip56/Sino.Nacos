using NLog;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Core
{
    /// <summary>
    /// 负载均衡算法
    /// </summary>
    public class Balancer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static IList<Instance> SelectAll(ServiceInfo serviceInfo)
        {
            if (serviceInfo.Hosts == null || serviceInfo.Hosts.Count <= 0)
            {
                throw new ArgumentNullException($"no host to srv for serviceInfo: {serviceInfo.Name}");
            }

            return serviceInfo.Hosts;
        }

        /// <summary>
        /// 根据权重获取
        /// </summary>
        public static Instance SelectHost(ServiceInfo serviceInfo)
        {
            var hosts = SelectAll(serviceInfo);

            _logger.Debug("entry randomWithWeight");

            Chooser<string, Instance> vipChooser = new Chooser<string, Instance>("www.vip56.cn");

            _logger.Debug("new Chooser");

            IList<Pair<Instance>> hostsWithWeight = new List<Pair<Instance>>();
            foreach(var host in hosts)
            {
                if (host.Healthy)
                {
                    hostsWithWeight.Add(new Pair<Instance>(host, host.Weight));
                }
            }

            _logger.Debug("foreach (Host host in hosts)");
            vipChooser.Refresh(hostsWithWeight);
            _logger.Debug("vipChooser.Refresh");
            return vipChooser.RandomWithWeight();
        }
    }
}
