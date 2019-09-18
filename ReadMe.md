# Sino.Nacos
该项目将参考Nacos的[java客户端](https://github.com/alibaba/nacos/tree/develop/client)进行改写，便于直接使用该技术实现服务的注册与发现。

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

## 额外说明

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
