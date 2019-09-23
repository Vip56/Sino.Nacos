# Sino.Nacos
该项目将参考Nacos的[java客户端](https://github.com/alibaba/nacos/tree/develop/client)进行改写，便于直接使用该技术实现服务的注册与发现。

[![Build status](https://ci.appveyor.com/api/projects/status/hwvasqhqpu48u4pk/branch/master?svg=true)](https://ci.appveyor.com/project/vip56/sino-nacos/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg?style=plastic)](https://www.nuget.org/packages/Sino.Nacos.Naming)   

## 使用方式

### 入门操作
首先需要使用者在通过Nuget引用该类库：
```
Install-Package Sino.Nacos.Naming -Version 1.0.0-beta1
```

完成引用后在配置文件中按照如下进行配置的设置：
```
"NacosNaming": {
  "ServerAddr": [ "http://localhost:8848" ],
  "CacheDir": "G:\\SinoGithub\\sinonacos\\Nacos",
}
```

然后回到`Startup`中将服务注入：
```
public void ConfigureServices(IServiceCollection services)
{
   services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
   services.AddHttpClient();
   // 以下代码
   services.AddNacosNaming(Configuration.GetSection("NacosNaming"));
}
```

完成服务的注入后我们通过`INamingService`接口就可以进行对应的操作。

### 自动注册
如果需要启用自动注入，可以通过配置以及对应代码开启，就可以实现服务启动后自行根据配置进行服务的注册，首先我们需要
修改对应的配置增加几个额外的配置项：
```
  "NacosNaming": {
    "Namespace": "73f7c6cb-4053-4d95-a476-44f5b5dafc34",
    "ServerAddr": [ "http://localhost:8848" ],
    "CacheDir": "G:\\SinoGithub\\sinonacos\\Nacos",
    "AutoRegister": true,
    "IpPrefix": "192.168.2",
    "ServiceName": "tms_order",
    "Port": 5001
  }
```
其中`AutoRegister`必须为`True`，而`IpPrefix`则是在实际服务存在多个地址的情况下进行前缀匹配从而决定注册哪个地址
，剩下的两个配置则代表服务的名称和对应的端口。

完成以上配置后还需要在`Startup`中进行如下几个配置：
```
public void ConfigureServices(IServiceCollection services)
{
   services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
   services.AddHttpClient();
   services.AddNacosNaming(Configuration.GetSection("NacosNaming"));
}
```
完成服务的注入后还需要启动自动注册：
```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  if (env.IsDevelopment())
  {
     app.UseDeveloperExceptionPage();
  }

  app.UseMvc();
  //以下代码将启用
  app.AutoRegisterNacosNaming();
}
```

### 服务接口
以下为服务的接口提供的完整功能：
```
    public interface INamingService
    {
        /// <summary>
        /// 根据配置自动注册
        /// </summary>
        Task AutoRegister();

        /// <summary>
        /// 注册一个实例到服务中
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        Task RegisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// 注册一个实例到服务中
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分株名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// 注册一个实例到服务，并指定具体的集群名称
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        Task RegisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// 注册一个实例到服务，并指定具体的集群名称
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// 注册一个实例到服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="instance">实例对象</param>
        Task RegisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// 注册一个实例到服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="instance">实例对象</param>
        Task RegisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        Task DeregisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        Task DeregisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="ip">实例IP</param>
        /// <param name="port">实例端口</param>
        /// <param name="clusterName">集群名称</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="instance">实例对象</param>
        Task DeregisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// 从服务中注销一个实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="instance">实例对象</param>
        Task DeregisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// 获取服务中所有实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        Task<IList<Instance>> GetAllInstances(string serviceName);

        /// <summary>
        /// 获取服务中所有实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 获取服务中的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        Task<IList<Instance>> SelectInstances(string serviceName, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// 从服务中获取可用的实例列表
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="healthy">决定获取健康还是非健康的实例</param>
        /// <param name="subscribe">是否订阅</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="subscribe">是否订阅</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="subscribe">是否订阅</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <exception cref="ArgumentNullException">如果服务不存在则会抛出该异常</exception>
        Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否为已订阅</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 通过预定义的负载均衡策略选取一个健康的实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="subscribe">是否已订阅</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="listener">监听</param>
        Task Subscribe(string serviceName, Action<IEvent> listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="listener">监听</param>
        Task Subscribe(string serviceName, string groupName, Action<IEvent> listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        Task Subscribe(string serviceName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        Task Subscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, Action<IEvent> listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, string groupName, Action<IEvent> listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// 注销订阅
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="clusters">集群名称列表</param>
        /// <param name="listener">监听</param>
        void Unsubscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        /// <param name="groupName">分组名称</param>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName);

        /// <summary>
        /// 获取所有服务的名称
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <param name="selector">选择器</param>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName, Selector selector);

        /// <summary>
        /// 获取所有已经订阅的服务信息
        /// </summary>
        IList<ServiceInfo> GetSubscribeServices();

        /// <summary>
        /// 获取服务状态
        /// </summary>
        Task<string> GetServerStatus();
```

## 功能说明

### 灾备支持
考虑到网络和Nacos服务的故障，当出现服务出现长时间无法通信的情况。内部通过定时获取服务最新的数据并保存在`cacheDir`路径下的`failover`目录下，
如果开发者需要开启进行切换当前需要开发者自行通过写入灾备目录下的`00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00`文件进行控制，并且需要
在启动SDK前在指定目录下创建好文件并写入对应值，通过"0"和"1"控制，其中"1"代表开启，一旦开启所有获取服务将通过灾备缓存的数据而不会请求服务端程序。    

### 服务心跳
考虑到实际使用该SDK的场景中可能会出现相同程序同时注册不同的服务名称或注册不同服务名称且出口IP和端口不一样的情况，所以该SDK实现了基于
服务名称+IP+Port级别的服务心跳以保证Nacos注册中心不会将我们注册的服务置为不健康状态，但是这也意味着注册过多将会导致一定的CPU的消耗。    

### 实例个性化
当使用该SDK进行服务注册操作，除了基本的参数外，通过设定实例的具体参数可以额外控制一些其他属性，只有通过`Instance.MetaData`才可以，主要
支持的属性如下：
```
/// <summary>
/// 提供实例附加数据
/// </summary>
public class PreservedMetadataKeys
{
    /// <summary>
    /// 注册来源
    /// </summary>
    public const string REGISTER_SOURCE = "preserved.register.source";

    /// <summary>
    /// 心跳超时时间
    /// </summary>
    public const string HEART_BEAT_TIMEOUT = "preserved.heart.beat.timeout";

    /// <summary>
    /// 实例移除超时时间
    /// </summary>
    public const string IP_DELETE_TIMEOUT = "preserved.ip.delete.timeout";

    /// <summary>
    /// 心跳间隔时间
    /// </summary>
    public const string HEART_BEAT_INTERVAL = "preserved.heart.beat.interval";
    }
```

### 启动Nacos服务端    
这里建议使用者参考官网文档基于Docker启动该[服务](https://nacos.io/zh-cn/docs/quick-start-docker.html)

## 额外说明

### 使用注意
1. 在进行服务注册时候如果需要自定义命名空间，需要首先在Nacos服务端创建一个新的命名空间然后将空间的Id（GUID）做为本SDK的namespace参数而不是名字;    
2. 

### 暂不支持功能
通过改写的过程中考虑到其情况部分功能进行了适当的删减和阉割，但是为了保证和官方的其他SDK之间的数据保持一致，这部分完全参考官方SDK进行设计开发。
其中主要有以下功能进行的删减或计划未来支持：    
* 通过UDP协议刷新服务信息；
* HTTP失败重试；
* 配置服务；

### 未来计划支持
1. 支持失败重试；
2. 支持多级缓存，而非单纯依靠文件；
3. 支持通过断路器实现智能灾备支持；

### 依赖类库
1. [mockhttp](https://github.com/richardszalay/mockhttp)：实现HttpClient模拟；
2. [moq](https://github.com/moq/moq4)：实现基本接口服务Mock；
3. [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)：Json序列化支持；
4. [Nlog](https://github.com/NLog/NLog)：提供日志记录；
5. [xunit](https://github.com/xunit/xunit)：单元测试框架；

### 依赖版本
只支持Asp.Net Core 2.1+项目

### 修订记录
* 19.9.18 完成NamingProxy与EventDispatcher单元测试;
