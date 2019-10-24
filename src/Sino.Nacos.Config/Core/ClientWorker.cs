﻿using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Sino.Nacos.Config.Common;
using Sino.Nacos.Config.Filter;
using Sino.Nacos.Config.Net;
using Sino.Nacos.Config.Exceptions;
using System.Threading.Tasks;
using System.IO;
using Sino.Nacos.Config.Utils;
using System.Net;

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
        private double _currentLongingTaskCount = 0;
        private long _timeout;

        public bool IsHealthServer { get; set; }

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

        public async Task<CacheData> AddCacheDataIfAbsent(string dataId, string group, string tenant)
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
                    string content = await GetServerConfig(dataId, group, tenant);
                    cache.Content = content;
                }
            }

            _cacheMap.AddOrUpdate(key, cache, (k, v) => cache);

            _logger.Info($"[{_agent.GetName()}] [subscribe] {key}");

            return cache;
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public async Task<string> GetServerConfig(string dataId, string group, string tenant)
        {
            if (string.IsNullOrEmpty(group))
                group = Constants.DEFAULT_GROUP;

            try
            {
                var param = new Dictionary<string, string>();
                param.Add("dataId", dataId);
                param.Add("group", group);
                if (!string.IsNullOrEmpty(tenant))
                {
                    param.Add("tenant", tenant);
                }

                string result = await _agent.Get(Constants.CONFIG_CONTROLLER_PATH, null, param);
                _localConfigInfoProcessor.SaveSnapshot(_agent.GetName(), dataId, group, tenant, result);

                return result;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[{_agent.GetName()}] [sub-server] get server config exception, dataId={dataId}, group={group}, tenant={tenant}");
                throw new NacosException(NacosException.SERVER_ERROR, ex);
            }
        }

        /// <summary>
        /// 校验本地缓存配置
        /// </summary>
        private void CheckLocalConfig(CacheData cacheData)
        {
            string dataId = cacheData.DataId;
            string group = cacheData.Group;
            string tenant = cacheData.Tenant;
            string path = _localConfigInfoProcessor.GetFailoverPath(_agent.GetName(), dataId, group, tenant);

            if (!cacheData.IsUseLocalConfig && File.Exists(path))
            {
                string content = _localConfigInfoProcessor.GetFailover(_agent.GetName(), dataId, group, tenant);
                string md5 = CacheData.GetMD5String(content);
                cacheData.IsUseLocalConfig = true;
                cacheData.LocalConfigLastModified = File.GetLastWriteTime(path).GetTimeStamp();
                cacheData.Content = content;

                _logger.Warn($"[{_agent.GetName()}] [failover-change] failover file created. dataId={dataId}, group={group}, tenant={tenant}, md5={md5}, content={ContentUtils.TruncateContent(content)}");
                return;
            }

            if (cacheData.IsUseLocalConfig && !File.Exists(path))
            {
                cacheData.IsUseLocalConfig = false;
                _logger.Warn($"[{_agent.GetName()}] [failover-change] failover file deleted, dataId={dataId}, group={group}, tenant={tenant}");
                return;
            }

            if (cacheData.IsUseLocalConfig && File.Exists(path) && cacheData.LocalConfigLastModified != File.GetLastWriteTime(path).GetTimeStamp())
            {
                string content = _localConfigInfoProcessor.GetFailover(_agent.GetName(), dataId, group, tenant);
                string md5 = CacheData.GetMD5String(content);
                cacheData.IsUseLocalConfig = true;
                cacheData.LocalConfigLastModified = File.GetLastWriteTime(path).GetTimeStamp();
                cacheData.Content = content;
                _logger.Warn($"[{_agent.GetName()}] [failover-change] failover file changed. dataId={dataId}, group={group}, tenant={tenant}, md5={md5}, content={ContentUtils.TruncateContent(content)}");
            }
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

        public void CheckConfigInfo()
        {
            int listenerSize = _cacheMap.Count;
            int longingTaskCount = (int)Math.Ceiling(listenerSize / UtilAndComs.PER_TASK_CONFIG_SIZE);
            if (longingTaskCount > _currentLongingTaskCount)
            {
                for (int i = (int)_currentLongingTaskCount; i < longingTaskCount; i++)
                {
                    // 后台线程任务
                }
                _currentLongingTaskCount = longingTaskCount;
            }
        }

        private IList<string> CheckUpdateDataIds(IList<CacheData> cacheDatas, IList<string> inInitializingCacheList)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var cacheData in cacheDatas)
            {
                if(!cacheData.IsUseLocalConfig)
                {

                }
            }
            bool isInitializingCacheList = inInitializingCacheList.Count >= 0;
            
        }

        private async Task<IList<string>> CheckUpdateConfigStr(string probeUpdateString, bool isInitializingCacheList)
        {
            var paramValues = new Dictionary<string, string>();
            paramValues.Add(Constants.PROBE_MODIFY_REQUEST, probeUpdateString);

            var headers = new Dictionary<string, string>();
            headers.Add("Long-Pulling-Timeout", _timeout.ToString());

            if (isInitializingCacheList)
            {
                headers.Add("Long-Pulling-Timeout-No-Hangup", "true");
            }

            if (string.IsNullOrEmpty(probeUpdateString))
            {
                return null;
            }

            try
            {
                string result = await _agent.Post(Constants.CONFIG_CONTROLLER_PATH + "/listener", headers, paramValues);

                if (string.IsNullOrEmpty(result))
                {
                    IsHealthServer = false;
                    _logger.Error($"[{_agent.GetName()}] [check-update] get changed dataId error.");
                }
                else
                {
                    IsHealthServer = true;
                    return ParseUpdateDataIdResponse(result);
                }
            }
            catch(Exception ex)
            {
                IsHealthServer = false;
                _logger.Error(ex, $"[{_agent.GetName()}] [check-update] get changed dataId exception");
                throw ex;
            }
            return null;
        }

        private IList<string> ParseUpdateDataIdResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            response = WebUtility.UrlDecode(response);

            var updateList = new List<string>();

            foreach(string dataIdAndGroup in response.Split(Constants.LINE_SEPARATOR))
            {
                if (!string.IsNullOrEmpty(dataIdAndGroup))
                {
                    var keyArr = dataIdAndGroup.Split(Constants.WORD_SEPARATOR);
                    string dataId = keyArr[0];
                    string group = keyArr[1];
                    if (keyArr.Length == 2)
                    {
                        updateList.Add(GroupKey.GetKey(dataId, group));
                        _logger.Info($"[{_agent.GetName()}] [polling-resp] config changed. dataId={dataId}, group={group}");
                    }
                    else if(keyArr.Length == 3)
                    {

                    }
                }
            }
        }
    }
}
