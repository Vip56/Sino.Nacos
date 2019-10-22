using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Sino.Nacos.Config.Common;
using Sino.Nacos.Config.Filter;
using Sino.Nacos.Config.Net;

namespace Sino.Nacos.Config.Core
{
    /// <summary>
    /// 长轮询任务
    /// </summary>
    public class ClientWorker
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<string, CacheData> _cacheMap = new ConcurrentDictionary<string, CacheData>();
        private ConfigFilterChainManager _configFilterChainManager;
        private LocalConfigInfoProcessor _localConfigInfoProcessor;
        private IHttpAgent _agent;
        private bool _enableRemoteSyncConfig = false;

        public void AddListeners(string dataId, string group, IList<Action<string>> listeners)
        {
            group = Null2DefaultGroup(group);
        }

        public CacheData AddCacheDataIfAbsent(string dataId, string group)
        {
            var cache = GetCache(dataId, group);
            if (cache != null)
                return cache;

            string key = GroupKey.GetKey(dataId, group);
            cache = new CacheData(_configFilterChainManager, _localConfigInfoProcessor, _agent.GetName(), dataId, group);

            var cacheFromMap = GetCache(dataId, group);
            if (cacheFromMap != null)
            {
                cache = cacheFromMap;
                cache.IsInitializing = true;
            }
            else
            {
                int taskId = _cacheMap.Count / (int)UtilAndComs.PER_TASK_CONFIG_SIZE;
                cache.TaskId = taskId;
            }

            _cacheMap.AddOrUpdate(key, cache, (k, v) => cache);

            _logger.Info($"[{_agent.GetName()}] [subscribe] {key}");

            return cache;
        }

        public CacheData AddCacheDataIfAbsent(string dataId, string group, string tenant)
        {
            var cache = GetCache(dataId, group, tenant);
            if (cache != null)
                return cache;

            string key = GroupKey.GetKeyTenant(dataId, group, tenant);
            var cacheFromMap = GetCache(dataId, group, tenant);

            if (cacheFromMap != null)
            {
                cache = cacheFromMap;
                cache.IsInitializing = true;
            }
            else
            {
                cache = new CacheData(_configFilterChainManager, _localConfigInfoProcessor, _agent.GetName(), dataId, group, tenant);
                if (_enableRemoteSyncConfig)
                {
                    string content = GetServerConfig(dataId, group, tenant, 3000L);
                    cache.Content = content;
                }
            }

            _cacheMap.AddOrUpdate(key, cache, (k, v) => cache);

            _logger.Info($"[{_agent.GetName()}] [subscribe] {key}");

            return cache;
        }

        public string GetServerConfig(string dataId, string group, string tenant, long readTimeout)
        {
            if (string.IsNullOrEmpty(group))
                group = Constants.DEFAULT_GROUP;


        }

        public CacheData GetCache(string dataId, string group)
        {
            return GetCache(dataId, group, Constants.DEFAULT_TENANT_ID);
        }

        public CacheData GetCache(string dataId, string group, string tenant)
        {
            if (string.IsNullOrEmpty(dataId))
                throw new ArgumentNullException(nameof(dataId));
            if (string.IsNullOrEmpty(group))
                throw new ArgumentNullException(nameof(group));

            CacheData result = null;
            if (_cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out result))
            {
                return result;
            }
            return result;
        }

        private string Null2DefaultGroup(string group)
        {
            return string.IsNullOrEmpty(group) ? Constants.DEFAULT_GROUP : group.Trim();
        }
    }
}
