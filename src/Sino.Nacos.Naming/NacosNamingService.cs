using Sino.Nacos.Naming.Beat;
using Sino.Nacos.Naming.Core;
using Sino.Nacos.Naming.Listener;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sino.Nacos.Naming
{
    /// <summary>
    /// 服务注册发现实现
    /// </summary>
    public class NacosNamingService : INamingService
    {
        public const int DEFAULT_HEART_BEAT_INTERVAL = 5 * 1000;
        public const string DEFAULT_GROUP = "DEFAULT_GROUP";
        public const string DEFAULT_CLUSTER_NAME = "DEFAULT";

        private HostReactor _hostReactor;
        private BeatReactor _beatReactor;
        private EventDispatcher _eventDispatcher;
        private NamingProxy _namingProxy;
        private NamingConfig _config;

        public NacosNamingService(NamingConfig config, IHttpClientFactory httpClientFactory)
        {
            _config = config;

            _eventDispatcher = new EventDispatcher();
            _namingProxy = new NamingProxy(config, new FastHttp(httpClientFactory, config));
            _beatReactor = new BeatReactor(_namingProxy);
            _hostReactor = new HostReactor(_eventDispatcher, _namingProxy, config.CacheDir, config.LoadCacheAtStart);
        }

        private string GetGroupedName(string serviceName, string groupName)
        {
            return groupName + Constants.SERVICE_INFO_SPLITER + serviceName;
        }

        public BeatReactor GetBeatReactor()
        {
            return _beatReactor;
        }

        public async Task DeregisterInstance(string serviceName, string ip, int port)
        {
            await DeregisterInstance(serviceName, ip, port, DEFAULT_CLUSTER_NAME);
        }

        public async Task DeregisterInstance(string serviceName, string groupName, string ip, int port)
        {
            await DeregisterInstance(serviceName, groupName, ip, port, DEFAULT_CLUSTER_NAME);
        }

        public async Task DeregisterInstance(string serviceName, string ip, int port, string clusterName)
        {
            await DeregisterInstance(serviceName, DEFAULT_GROUP, ip, port, clusterName);
        }

        public async Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName)
        {
            Instance instance = new Instance();
            instance.Ip = ip;
            instance.Port = port;
            instance.ClusterName = clusterName;

            await DeregisterInstance(serviceName, groupName, instance);
        }

        public async Task DeregisterInstance(string serviceName, Instance instance)
        {
            await DeregisterInstance(serviceName, DEFAULT_GROUP, instance);
        }

        public async Task DeregisterInstance(string serviceName, string groupName, Instance instance)
        {
            if (instance.Ephemeral)
            {
                _beatReactor.RemoveBeatInfo(GetGroupedName(serviceName, groupName), instance.Ip, instance.Port);
            }
            await _namingProxy.DeregisterService(GetGroupedName(serviceName, groupName), instance);
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName)
        {
            return GetAllInstances(serviceName, new List<string>());
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, string groupName)
        {
            return GetAllInstances(serviceName, groupName, new List<string>());
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, bool subscribe)
        {
            return GetAllInstances(serviceName, new List<string>(), subscribe);
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe)
        {
            return GetAllInstances(serviceName, groupName, new List<string>(), subscribe);
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters)
        {
            return GetAllInstances(serviceName, clusters, true);
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters)
        {
            return GetAllInstances(serviceName, groupName, clusters, true);
        }

        public Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters, bool subscribe)
        {
            return GetAllInstances(serviceName, DEFAULT_GROUP, clusters, subscribe);
        }

        public async Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters, bool subscribe)
        {
            ServiceInfo serviceInfo = null;
            if (subscribe)
            {
                serviceInfo = await _hostReactor.GetServiceInfo(GetGroupedName(serviceName, groupName), string.Join(",", clusters));
            }
            else
            {
                serviceInfo = await _hostReactor.GetServiceInfoDirectlyFromServer(GetGroupedName(serviceName, groupName), string.Join(",", clusters));
            }
            if (serviceInfo == null || serviceInfo.Hosts.Count <= 0)
            {
                return new List<Instance>();
            }
            return serviceInfo.Hosts;
        }

        public async Task<string> GetServerStatus()
        {
            return await _namingProxy.ServerHealthy() ? "UP" : "DOWN";
        }

        public Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize)
        {
            return GetServicesOfServer(pageNo, pageSize, DEFAULT_GROUP);
        }

        public Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName)
        {
            return GetServicesOfServer(pageNo, pageSize, groupName, null);
        }

        public async Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName, Selector selector)
        {
            return await _namingProxy.GetServiceList(pageNo, pageSize, groupName, selector);
        }

        public IList<ServiceInfo> GetSubscribeServices()
        {
            return _eventDispatcher.GetSubscribeServices();
        }

        public async Task RegisterInstance(string serviceName, string ip, int port)
        {
            await RegisterInstance(serviceName, ip, port, DEFAULT_CLUSTER_NAME);
        }

        public async Task RegisterInstance(string serviceName, string groupName, string ip, int port)
        {
            await RegisterInstance(serviceName, groupName, ip, port, DEFAULT_CLUSTER_NAME);
        }

        public async Task RegisterInstance(string serviceName, string ip, int port, string clusterName)
        {
            await RegisterInstance(serviceName, DEFAULT_GROUP, ip, port, clusterName);
        }

        public async Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName)
        {
            Instance instance = new Instance();
            instance.Ip = ip;
            instance.Port = port;
            instance.Weight = 1;
            instance.ClusterName = clusterName;

            await RegisterInstance(serviceName, groupName, instance);
        }

        public async Task RegisterInstance(string serviceName, Instance instance)
        {
            await RegisterInstance(serviceName, DEFAULT_GROUP, instance);
        }

        public async Task RegisterInstance(string serviceName, string groupName, Instance instance)
        {
            if (instance.Ephemeral)
            {
                BeatInfo beatInfo = new BeatInfo();
                beatInfo.ServiceName = GetGroupedName(serviceName, groupName);
                beatInfo.Ip = instance.Ip;
                beatInfo.Port = instance.Port;
                beatInfo.Cluster = instance.ClusterName;
                beatInfo.Weight = instance.Weight;
                beatInfo.MetaData = instance.Metadata;
                beatInfo.Scheduled = false;
                beatInfo.PerId = instance.GetInstanceHeartBeatInterval();

                _beatReactor.AddBeatInfo(GetGroupedName(serviceName, groupName), beatInfo);
            }

            await _namingProxy.RegisterService(GetGroupedName(serviceName, groupName), groupName, instance);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, bool healthy)
        {
            return SelectInstances(serviceName, new List<string>(), healthy);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy)
        {
            return SelectInstances(serviceName, groupName, healthy, true);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe)
        {
            return SelectInstances(serviceName, new List<string>(), healthy, subscribe);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe)
        {
            return SelectInstances(serviceName, groupName, new List<string>(), healthy, subscribe);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy)
        {
            return SelectInstances(serviceName, clusters, healthy, true);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy)
        {
            return SelectInstances(serviceName, groupName, clusters, healthy, true);
        }

        public Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy, bool subscribe)
        {
            return SelectInstances(serviceName, DEFAULT_GROUP, clusters, healthy, subscribe);
        }

        public async Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy, bool subscribe)
        {
            ServiceInfo serviceInfo = null;
            if (subscribe)
            {
                serviceInfo = await _hostReactor.GetServiceInfo(GetGroupedName(serviceName, groupName), string.Join(",", clusters));
            }
            else
            {
                serviceInfo = await _hostReactor.GetServiceInfoDirectlyFromServer(GetGroupedName(serviceName, groupName), string.Join(",", clusters));
            }
            
            if (serviceInfo == null || serviceInfo.Hosts.Count <= 0)
            {
                return new List<Instance>();
            }

            var list = new List<Instance>();
            foreach(var item in serviceInfo.Hosts)
            {
                if (healthy == item.Healthy || item.Enable || item.Weight > 0)
                    list.Add(item);
            }
            return list;
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName)
        {
            return SelectOneHealthyInstance(serviceName, new List<string>());
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName)
        {
            return SelectOneHealthyInstance(serviceName, groupName, true);
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe)
        {
            return SelectOneHealthyInstance(serviceName, new List<string>(), subscribe);
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe)
        {
            return SelectOneHealthyInstance(serviceName, groupName, new List<string>(), subscribe);
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters)
        {
            return SelectOneHealthyInstance(serviceName, clusters, true);
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters)
        {
            return SelectOneHealthyInstance(serviceName, groupName, clusters, true);
        }

        public Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters, bool subscribe)
        {
            return SelectOneHealthyInstance(serviceName, DEFAULT_GROUP, clusters, subscribe);
        }

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters, bool subscribe)
        {
            if (subscribe)
            {
                return Balancer.SelectHost(await _hostReactor.GetServiceInfo(GetGroupedName(serviceName, groupName), string.Join(",", clusters)));
            }
            else
            {
                return Balancer.SelectHost(await _hostReactor.GetServiceInfoDirectlyFromServer(GetGroupedName(serviceName, groupName), string.Join(",", clusters)));
            }
        }

        public async Task Subscribe(string serviceName, Action<IEvent> listener)
        {
            await Subscribe(serviceName, new List<string>(), listener);
        }

        public async Task Subscribe(string serviceName, string groupName, Action<IEvent> listener)
        {
            await Subscribe(serviceName, groupName, new List<string>(), listener);
        }

        public async Task Subscribe(string serviceName, IList<string> clusters, Action<IEvent> listener)
        {
            await Subscribe(serviceName, DEFAULT_GROUP, clusters, listener);
        }

        public async Task Subscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener)
        {
            _eventDispatcher.AddListener(await _hostReactor.GetServiceInfo(GetGroupedName(serviceName, groupName), string.Join(",", clusters)), string.Join(",", clusters), listener);
        }

        public void Unsubscribe(string serviceName, Action<IEvent> listener)
        {
            Unsubscribe(serviceName, new List<string>(), listener);
        }

        public void Unsubscribe(string serviceName, string groupName, Action<IEvent> listener)
        {
            Unsubscribe(serviceName, groupName, new List<string>(), listener);
        }

        public void Unsubscribe(string serviceName, IList<string> clusters, Action<IEvent> listener)
        {
            Unsubscribe(serviceName, DEFAULT_GROUP, clusters, listener);
        }

        public void Unsubscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener)
        {
            _eventDispatcher.RemoveListener(GetGroupedName(serviceName, groupName), string.Join(",", clusters), listener);
        }
    }
}
