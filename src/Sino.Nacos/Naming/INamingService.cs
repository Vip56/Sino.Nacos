using Sino.Nacos.Naming.Listener;
using Sino.Nacos.Naming.Model;
using System.Collections.Generic;

namespace Sino.Nacos.Naming
{
    public interface INamingService
    {
        /// <summary>
        /// 注册一个实例到服务中
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        void RegisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// 注册一个实例到服务中
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分株名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        void RegisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// 注册一个实例到服务，并指定具体的集群名称
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        void RegisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// 注册一个实例到服务，并指定具体的集群名称
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        void RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// 注册一个实例到服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="instance">实例对象</param>
        void RegisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// 注册一个实例到服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="instance">实例对象</param>
        void ReregisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        void DeregisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        void DeregisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        void DeregisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        void DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="instance">实例对象</param>
        void DeregisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="instance">实例对象</param>
        void DeregisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// 获取服务中所有实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        IList<Instance> GetAllInstances(string serviceName);

        /// <summary>
        /// 获取服务中所有实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        IList<Instance> GetAllInstances(string serviceName, string groupName);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> GetAllInstances(string serviceName, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> GetAllInstances(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        IList<Instance> GetAllInstances(string serviceName, IList<string> clusters);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        IList<Instance> GetAllInstances(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> GetAllInstances(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> GetAllInstances(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        IList<Instance> SelectInstances(string serviceName, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        IList<Instance> SelectInstances(string serviceName, string groupName, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> SelectInstances(string serviceName, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        IList<Instance> SelectInstances(string serviceName, IList<string> clusters, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        IList<Instance> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> SelectInstances(string serviceName, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupNamem">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否为订阅</param>
        IList<Instance> SelectInstances(string serviceName, string groupNamem, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        Instance SelectOneHealthyInstance(string serviceName);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        Instance SelectOneHealthyInstance(string serviceName, string groupName);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="subscribe">是否为订阅</param>
        Instance SelectOneHealthyInstance(string serviceName, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="subscribe">是否为订阅</param>
        Instance SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        Instance SelectOneHealthyInstance(string serviceName, IList<string> clusters);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        Instance SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否为已订阅</param>
        Instance SelectOneHealthyInstance(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否已订阅</param>
        Instance SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="listener">监听</param>
        void Subscribe(string serviceName, IEventListener listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="listener">监听</param>
        void Subscribe(string serviceName, string groupName, IEventListener listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Subscribe(string serviceName, IList<string> clusters, IEventListener listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Subscribe(string serviceName, string groupName, IList<string> clusters, IEventListener listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, IEventListener listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, string groupName, IEventListener listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, IList<string> clusters, IEventListener listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, string groupName, IList<string> clusters, IEventListener listener);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        ListView<string> GetServicesOfServer(int pageNo, int pageSize);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        /// <param name="groupName">分组名称</param>
        ListView<string> GetServicesOfServer(int pageNo, int pageSize, string groupName);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <param name="selector">选择器</param>
        ListView<string> GetServicesOfServer(int pageNo, int pageSize, string groupName, Selector selector);

        /// <summary>
        /// 获取所有已经订阅的服务信息
        /// </summary>
        IList<ServiceInfo> GetSubscribeServices();

        /// <summary>
        /// 获取服务状态
        /// </summary>
        string GetServerStatus();
    }
}
