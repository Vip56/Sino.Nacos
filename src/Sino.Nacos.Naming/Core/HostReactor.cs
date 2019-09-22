using Newtonsoft.Json;
using NLog;
using Sino.Nacos.Naming.Backups;
using Sino.Nacos.Naming.Cache;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Sino.Nacos.Naming.Core
{
    /// <summary>
    /// 实例维护
    /// </summary>
    public class HostReactor : IHostReactor
    {
        public static int DEFAULT_DELAY = 1000;
        public static int UPDATE_HOLD_INTERVAL = 5 * 1000;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<string, Timer> _timerMap = new ConcurrentDictionary<string, Timer>();
        private ConcurrentDictionary<string, ServiceInfo> _serviceInfoMap;
        private ConcurrentDictionary<string, Task> _updating = new ConcurrentDictionary<string, Task>();

        private EventDispatcher _eventDispatcher;
        private NamingProxy _namingProxy;
        private FailoverReactor _failoverReactor;
        private string _cacheDir;

        public HostReactor(EventDispatcher eventDispatcher, NamingProxy namingProxy, string cacheDir)
            : this(eventDispatcher, namingProxy, cacheDir, false) { }

        public HostReactor(EventDispatcher eventDispatcher, NamingProxy namingProxy, string cacheDir, bool loadCacheStart)
        {
            _eventDispatcher = eventDispatcher;
            _namingProxy = namingProxy;
            _cacheDir = cacheDir;
            if (loadCacheStart)
            {
                _serviceInfoMap = new ConcurrentDictionary<string, ServiceInfo>(DiskCache.GetServiceInfos(_cacheDir));
            }
            else
            {
                _serviceInfoMap = new ConcurrentDictionary<string, ServiceInfo>();
            }

            _failoverReactor = new FailoverReactor(this, _cacheDir);
        }

        public ConcurrentDictionary<string, ServiceInfo> GetServiceInfoMap()
        {
            return _serviceInfoMap;
        }

        public Task<ServiceInfo> GetServiceInfoDirectlyFromServer(string serviceName, string clusters)
        {
            return _namingProxy.QueryList(serviceName, clusters, 0, false);
        }

        public async Task<ServiceInfo> GetServiceInfo(string serviceName, string clusters)
        {
            _logger.Debug($"failover-mode: {_failoverReactor.IsFailoverSwitch()}");

            string key = ServiceInfo.GetKey(serviceName, clusters);
            if (_failoverReactor.IsFailoverSwitch())
            {
                return _failoverReactor.GetService(key);
            }

            ServiceInfo serviceObj = GetServiceInfoInner(serviceName, clusters);

            if (serviceObj == null)
            {
                Task wait;
                if (_updating.TryGetValue(key, out wait))
                {
                    wait.Wait();
                }
                else
                {
                    wait = UpdateServiceNow(serviceName, clusters);
                    _updating.TryAdd(key, wait);
                    await wait;
                }
            }

            ScheduleUpdateIfAbsent(serviceName, clusters);

            return _serviceInfoMap[key];
        }

        /// <summary>
        /// 添加到主动更新清单中
        /// </summary>
        private void ScheduleUpdateIfAbsent(string serviceName, string clusters)
        {
            string key = ServiceInfo.GetKey(serviceName, clusters);
            if (_timerMap.ContainsKey(key))
            {
                return;
            }

            lock(_timerMap)
            {
                if (_timerMap.ContainsKey(key))
                {
                    return;
                }

                var t = UpdateTask(serviceName, clusters);
                _timerMap.TryAdd(key, t);
            }
        }

        /// <summary>
        /// 获取新服务信息并与缓存对比后返回最新
        /// </summary>
        public ServiceInfo ProcessServiceJson(ServiceInfo serviceInfo)
        {
            ServiceInfo oldService = null;
            _serviceInfoMap.TryGetValue(serviceInfo.GetKey(), out oldService);

            if (serviceInfo.Hosts == null || !serviceInfo.Validate())
            {
                return oldService;
            }

            bool changed = false;

            if (oldService != null)
            {
                if (oldService.LastRefTime > serviceInfo.LastRefTime)
                {
                    _logger.Warn($"out of date data received, old-t: {oldService.LastRefTime}, new-t: {serviceInfo.LastRefTime}");
                }

                _serviceInfoMap.AddOrUpdate(serviceInfo.GetKey(), serviceInfo, (k, v) => serviceInfo);

                var oldHostMap = oldService.Hosts.ToDictionary(x => x.ToInetAddr());
                var newHostMap = serviceInfo.Hosts.ToDictionary(x => x.ToInetAddr());

                var modHosts = newHostMap.Where(x => oldHostMap.ContainsKey(x.Key) && !x.Value.ToString().Equals(oldHostMap[x.Key].ToString())).Select(x => x.Value);
                var newHosts = newHostMap.Where(x => !oldHostMap.ContainsKey(x.Key)).Select(x => x.Value);
                var remvHosts = oldHostMap.Where(x => !newHostMap.ContainsKey(x.Key)).Select(x => x.Value);

                if (newHosts.Count() > 0)
                {
                    changed = true;
                    _logger.Info($"new ips ({newHosts.Count()}) service: {serviceInfo.GetKey()} -> {JsonConvert.SerializeObject(newHosts)}");
                }

                if (remvHosts.Count() > 0)
                {
                    changed = true;
                    _logger.Info($"removed ips ({remvHosts.Count()}) service: {serviceInfo.GetKey()} -> {JsonConvert.SerializeObject(remvHosts)}");
                }

                if (modHosts.Count() > 0)
                {
                    changed = true;
                    _logger.Info($"modified ips ({modHosts.Count()}) service: {serviceInfo.GetKey()} -> {JsonConvert.SerializeObject(modHosts)}");
                }

                if (newHosts.Count() > 0 || remvHosts.Count() > 0 || modHosts.Count() >0 )
                {
                    _eventDispatcher.ServiceChanged(serviceInfo);
                    DiskCache.WriteServiceInfo(_cacheDir, serviceInfo);
                }
            }
            else
            {
                changed = true;
                _logger.Info($"init new ips({serviceInfo.IpCount()}) service: {serviceInfo.GetKey()} -> {JsonConvert.SerializeObject(serviceInfo.Hosts)}");
                _serviceInfoMap.TryAdd(serviceInfo.GetKey(), serviceInfo);
                _eventDispatcher.ServiceChanged(serviceInfo);
                DiskCache.WriteServiceInfo(_cacheDir, serviceInfo);
            }

            if (changed)
            {
                _logger.Info($"current ips({serviceInfo.IpCount()}) service: {serviceInfo.GetKey()} -> {JsonConvert.SerializeObject(serviceInfo.Hosts)}");
            }

            return serviceInfo;
        }

        /// <summary>
        /// 立即更新服务信息
        /// </summary>
        private async Task UpdateServiceNow(string serviceName, string clusters)
        {
            try
            {
                var result = await _namingProxy.QueryList(serviceName, clusters, 0, false).ConfigureAwait(false);

                if (result != null)
                {
                    ProcessServiceJson(result);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[NA] failed to update serviceName: {serviceName}");
            }
        }

        private Timer UpdateTask(string serviceName, string clusters)
        {
            return new Timer(async x =>
            {
                var state = x as UpdateState;

                ServiceInfo serviceInfo = null;
                string key = ServiceInfo.GetKey(state.ServiceName, state.Clusters);
                Timer self = null;
                _timerMap.TryGetValue(key, out self);
                if (_serviceInfoMap.TryGetValue(key, out serviceInfo))
                {
                    if (serviceInfo.LastRefTime <= state.LastRefTime)
                    {
                        await UpdateServiceNow(state.ServiceName, state.Clusters);
                        _serviceInfoMap.TryGetValue(key, out serviceInfo);
                    }
                    else
                    {
                        try
                        {
                            await _namingProxy.QueryList(state.ServiceName, state.Clusters, 0, false);
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex, $"[NA] failed to update serviceName: {state.ServiceName}");
                        }
                    }
                }
                else
                {
                    await UpdateServiceNow(state.ServiceName, state.Clusters);
                    Timer t = null;
                    if (_timerMap.TryGetValue(key, out t))
                    {
                        t.Change(DEFAULT_DELAY, Timeout.Infinite);
                    }
                    return;
                }

                state.LastRefTime = serviceInfo.LastRefTime;
                self?.Change(serviceInfo.CacheMillis, Timeout.Infinite);

            }, new UpdateState(serviceName, clusters), DEFAULT_DELAY, Timeout.Infinite);
        }

        /// <summary>
        /// 直接获取内部服务
        /// </summary>
        private ServiceInfo GetServiceInfoInner(string serviceName, string clusters)
        {
            string key = ServiceInfo.GetKey(serviceName, clusters);

            ServiceInfo serviceInfo = null;
            _serviceInfoMap.TryGetValue(key, out serviceInfo);

            return serviceInfo;
        }

        public class UpdateState
        {
            public string ServiceName { get; set; }
            public string Clusters { get; set; }

            public long LastRefTime { get; set; } = long.MaxValue;

            public UpdateState(string serviceName, string clusters)
            {
                this.ServiceName = serviceName;
                this.Clusters = clusters;
            }
        }
    }
}
