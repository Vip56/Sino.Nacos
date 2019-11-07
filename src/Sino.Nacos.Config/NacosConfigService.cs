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
            _localConfigInfoProcessor = new LocalConfigInfoProcessor(config.LocalFileRoot);
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
            if (_worker.IsHealthServer)
            {
                return "UP";
            }
            else
            {
                return "DOWN";
            }
        }

        public Task<bool> PublishConfig(string dataId, string group, string content)
        {
            return PublishConfigInner(_namespace, dataId, group, null, null, null, content);
        }

        public Task<bool> RemoveConfig(string dataId, string group)
        {
            return RemoveConfigInner(_namespace, dataId, group, null);
        }

        public void RemoveListener(string dataId, string group, Action<string> listener)
        {
            _worker.RemoveTenantListener(dataId, group, listener);
        }

        private async Task<bool> PublishConfigInner(string tenant, string dataId, string group, string tag, 
            string appName, string betaIps, string content)
        {
            if (string.IsNullOrEmpty(dataId))
                throw new ArgumentNullException(nameof(dataId));
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException(nameof(content));

            group = Null2DefaultGroup(group);

            ConfigRequest cr = new ConfigRequest();
            cr.DataId = dataId;
            cr.Tenant = tenant;
            cr.Group = group;
            cr.Content = content;
            _configFilterChainManager.DoFilter(cr, null);
            content = cr.Content;

            string url = Constants.CONFIG_CONTROLLER_PATH;
            var paramValue = new Dictionary<string, string>();
            paramValue.Add("dataId", dataId);
            paramValue.Add("group", group);
            paramValue.Add("content", content);

            if (!string.IsNullOrEmpty(tenant))
            {
                paramValue.Add("tenant", tenant);
            }
            if (!string.IsNullOrEmpty(appName))
            {
                paramValue.Add("appName", appName);
            }
            if (string.IsNullOrEmpty(tag))
            {
                paramValue.Add("tag", tag);
            }

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(betaIps))
            {
                headers.Add("betaIps", betaIps);
            }

            try
            {
                string result = await _agent.Post(url, headers, paramValue);
                if (string.IsNullOrEmpty(result))
                {
                    _logger.Warn($"[{_agent.GetName()}] [publish-single] error, dataId={dataId}, group={group}, tenant={tenant}");
                    return false;
                }
                _logger.Info($"[{_agent.GetName()}] [publish-single] ok, dataId={dataId}, group={group}, tenant={tenant}, config={ContentUtils.TruncateContent(content)}");
                return true;
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"[{_agent.GetName()}] [publish-single] error, dataId={dataId}, group={group}, tenant={tenant}");
                return false;
            }
        }

        private async Task<bool> RemoveConfigInner(string tenant, string dataId, string group, string tag)
        {
            if (string.IsNullOrEmpty(dataId))
                throw new ArgumentNullException(nameof(dataId));

            group = Null2DefaultGroup(group);
            string url = Constants.CONFIG_CONTROLLER_PATH;
            var paramValue = new Dictionary<string, string>();
            paramValue.Add("dataId", dataId);
            paramValue.Add("group", group);
            if (!string.IsNullOrEmpty(tenant))
            {
                paramValue.Add("tenant", tenant);
            }
            if (!string.IsNullOrEmpty(tag))
            {
                paramValue.Add("tag", tag);
            }

            try
            {
                string result = await _agent.Delete(url, null, paramValue);
                if (string.IsNullOrEmpty(result))
                {
                    _logger.Warn($"[{_agent.GetName()}] [remove] error, dataId={dataId}, group={group}, tenant={tenant}");
                    return false;
                }
                _logger.Info($"[{_agent.GetName()}] [remove] ok, dataId={dataId}, group={group}, tenant={tenant}");
                return true;
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"[{_agent.GetName()}] [remove] ok, dataId={dataId}, group={group}, tenant={tenant}");
                return false;
            }
        }

        private string Null2DefaultGroup(string group)
        {
            return string.IsNullOrEmpty(group) ? Constants.DEFAULT_GROUP : group.Trim();
        }
    }
}
