using NLog;
using Sino.Nacos.Config.Core;
using Sino.Nacos.Config.Filter;
using Sino.Nacos.Config.Net;
using Sino.Nacos.Config.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sino.Nacos.Config
{
    public class NacosConfigService : IConfigService
    {
        public static long POST_TIMEOUT = 3000L;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _namespace;
        private IHttpAgent _agent;
        private ClientWorker _worker;
        private FastHttp _http;
        private ConfigFilterChainManager _configFilterChainManager = new ConfigFilterChainManager();
        private LocalConfigInfoProcessor _localConfigInfoProcessor;

        public NacosConfigService(ConfigParam config, IHttpClientFactory factory)
        {
            _namespace = config.Namespace;
            _http = new FastHttp(factory, config);
            _agent = new ServerHttpAgent(config, _http);
            _localConfigInfoProcessor = new LocalConfigInfoProcessor(config);
            _worker = new ClientWorker(config, _agent, _configFilterChainManager, _localConfigInfoProcessor);
        }

        public Task AddListener(string dataId, string group, Action<string> listener)
        {
            return _worker.AddTenantListeners(dataId, group, listener);
        }

        public async Task<string> GetConfig(string dataId, string group)
        {
            if (string.IsNullOrEmpty(dataId))
                throw new ArgumentNullException(nameof(dataId));

            group = Null2DefaultGroup(group);

            ConfigResponse cr = new ConfigResponse();
            cr.DataId = dataId;
            cr.Tenant = _namespace;
            cr.Group = group;

            // 先使用本地缓存
            string content = _localConfigInfoProcessor.GetFailover(_agent.GetName(), dataId, group, _namespace);
            if (!string.IsNullOrEmpty(content))
            {
                _logger.Warn($"[{_agent.GetName()}] [get-config] get failover ok, dataId={dataId}, group={group}, tenant={_namespace}, config={ContentUtils.TruncateContent(content)}");
                cr.Content = content;
                _configFilterChainManager.DoFilter(null, cr);
                content = cr.Content;
                return content;
            }

            try
            {
                content = await _worker.GetServerConfig(dataId, group, _namespace);
                cr.Content = content;
                _configFilterChainManager.DoFilter(null, cr);
                content = cr.Content;
                return content;
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"[{_agent.GetName()}] [get-config] get from server error, dataId={dataId}, group={group}, tenant={_namespace}");
            }

            _logger.Warn($"[{_agent.GetName()}] [get-config] get snapshot ok, dataId={dataId}, tenant={_namespace}, config={ContentUtils.TruncateContent(content)}");
            content = _localConfigInfoProcessor.GetSnapshot(_agent.GetName(), dataId, group, _namespace);
            cr.Content = content;
            _configFilterChainManager.DoFilter(null, cr);
            content = cr.Content;
            return content;
        }

        public async Task<string> GetConfigAndSignListener(string dataId, string group, long timeout, Action<string> listener)
        {
            string content = await GetConfig(dataId, group);
            await _worker.AddTenantListenersWithContent(dataId, group, content, listener);
            return content;
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

        private string Null2DefaultGroup(string group)
        {
            return string.IsNullOrEmpty(group) ? Constants.DEFAULT_GROUP : group.Trim();
        }
    }
}
