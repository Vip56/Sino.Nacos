using NLog;
using Sino.Nacos.Config.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config
{
    public class NacosConfigService : IConfigService
    {
        public static long POST_TIMEOUT = 3000L;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IHttpAgent _agent;

        public string AddListener(string dataId, string group, Action<string> listener)
        {
            throw new NotImplementedException();
        }

        public string GetConfig(string dataId, string group, long timeout)
        {
            throw new NotImplementedException();
        }

        public string GetConfigAndSignListener(string dataId, string group, long timeout, Action<string> listener)
        {
            throw new NotImplementedException();
        }

        public string GetServerStatus()
        {
            throw new NotImplementedException();
        }

        public bool PublishConfig(string dataId, string group, string content)
        {
            throw new NotImplementedException();
        }

        public bool RemoveConfig(string dataId, string group)
        {
            throw new NotImplementedException();
        }

        public void RemoveListener(string dataId, string group, Action<string> listener)
        {
            throw new NotImplementedException();
        }
    }
}
