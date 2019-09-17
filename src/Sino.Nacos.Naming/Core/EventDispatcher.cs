using NLog;
using Sino.Nacos.Naming.Listener;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sino.Nacos.Naming.Core
{
    /// <summary>
    /// 事件分发
    /// </summary>
    public class EventDispatcher
    {
        public const int TAKE_WAIT_MILLISECONDS_TIMEOUT = 5 * 60 * 1000;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 服务变更列表
        /// </summary>
        private BlockingCollection<ServiceInfo> changedServices = new BlockingCollection<ServiceInfo>();

        /// <summary>
        /// 用户监听列表
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentList<Action<IEvent>>> observerMap = new ConcurrentDictionary<string, ConcurrentList<Action<IEvent>>>();

        public EventDispatcher()
        {
            Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    ServiceInfo info = null;
                    info = changedServices.Take();

                    if(changedServices.TryTake(out info, TAKE_WAIT_MILLISECONDS_TIMEOUT))
                    {
                        try
                        {
                            ConcurrentList<Action<IEvent>> listeners = null;
                            if (observerMap.TryGetValue(info.GetKey(), out listeners))
                            {
                                if (listeners != null || listeners.Count > 0)
                                {
                                    var hosts = new ReadOnlyCollection<Instance>(info.Hosts);
                                    listeners.ForEach(x =>
                                    {
                                        x.Invoke(new NamingEvent(info.Name, info.GroupName, info.Clusters, hosts));
                                    });
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex, $"[NA] notify error for service: {info.Name}, clusters: {info.Clusters}");
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            });
        }

        /// <summary>
        /// 添加订阅
        /// </summary>
        public void AddListener(ServiceInfo serviceInfo, string clusters, Action<IEvent> listener)
        {
            _logger.Info($"[LISTENER] adding {serviceInfo.Name} with {clusters} to listener map");

            var observers = new ConcurrentList<Action<IEvent>>();
            observers.Add(listener);

            observers = observerMap.GetOrAdd(ServiceInfo.GetKey(serviceInfo.Name, clusters), s => observers);
            if (observers != null)
            {
                observers.Add(listener);
            }

        }

        /// <summary>
        /// 注销订阅
        /// </summary>
        public void RemoveListener(string serviceName, string clusters, Action<IEvent> listener)
        {
            _logger.Info($"[LISTENER] removing {serviceName} with {clusters} from listener key");

            ConcurrentList<Action<IEvent>> observers = null;
            if (observerMap.TryGetValue(ServiceInfo.GetKey(serviceName, clusters), out observers))
            {
                observers.Remove(listener);
                if (observers.Count <= 0)
                {
                    observerMap.TryRemove(ServiceInfo.GetKey(serviceName, clusters), out observers);
                }
            }
        }

        /// <summary>
        /// 获取订阅列表
        /// </summary>
        public IList<ServiceInfo> GetSubscribeServices()
        {
            IList<ServiceInfo> serviceInfos = new List<ServiceInfo>();
            foreach(var key in observerMap.Keys)
            {
                serviceInfos.Add(ServiceInfo.FromKey(key));
            }
            return serviceInfos;
        }

        /// <summary>
        /// 触发服务变更
        /// </summary>
        public void ServiceChanged(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return;
            }

            changedServices.Add(serviceInfo);
        }
    }
}
