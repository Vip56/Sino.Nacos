# Sino.Nacos
����Ŀ���ο�Nacos��[java�ͻ���](https://github.com/alibaba/nacos/tree/develop/client)���и�д������ֱ��ʹ�øü���ʵ�ַ����ע���뷢�֡�

[![Build status](https://ci.appveyor.com/api/projects/status/hwvasqhqpu48u4pk/branch/master?svg=true)](https://ci.appveyor.com/project/vip56/sino-nacos/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg?style=plastic)](https://www.nuget.org/packages/Sino.Nacos.Naming)   

## ʹ�÷�ʽ

### ���Ų���
������Ҫʹ������ͨ��Nuget���ø���⣺
```
Install-Package Sino.Nacos.Naming -Version 1.0.0-beta1
```

������ú��������ļ��а������½������õ����ã�
```
"NacosNaming": {
  "ServerAddr": [ "http://localhost:8848" ],
  "CacheDir": "G:\\SinoGithub\\sinonacos\\Nacos",
}
```

Ȼ��ص�`Startup`�н�����ע�룺
```
public void ConfigureServices(IServiceCollection services)
{
   services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
   services.AddHttpClient();
   // ���´���
   services.AddNacosNaming(Configuration.GetSection("NacosNaming"));
}
```

��ɷ����ע�������ͨ��`INamingService`�ӿھͿ��Խ��ж�Ӧ�Ĳ�����

### �Զ�ע��
�����Ҫ�����Զ�ע�룬����ͨ�������Լ���Ӧ���뿪�����Ϳ���ʵ�ַ������������и������ý��з����ע�ᣬ����������Ҫ
�޸Ķ�Ӧ���������Ӽ�������������
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
����`AutoRegister`����Ϊ`True`����`IpPrefix`������ʵ�ʷ�����ڶ����ַ������½���ǰ׺ƥ��Ӷ�����ע���ĸ���ַ
��ʣ�µ���������������������ƺͶ�Ӧ�Ķ˿ڡ�

����������ú���Ҫ��`Startup`�н������¼������ã�
```
public void ConfigureServices(IServiceCollection services)
{
   services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
   services.AddHttpClient();
   services.AddNacosNaming(Configuration.GetSection("NacosNaming"));
}
```
��ɷ����ע�����Ҫ�����Զ�ע�᣺
```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  if (env.IsDevelopment())
  {
     app.UseDeveloperExceptionPage();
  }

  app.UseMvc();
  //���´��뽫����
  app.AutoRegisterNacosNaming();
}
```

### ����ӿ�
����Ϊ����Ľӿ��ṩ���������ܣ�
```
    public interface INamingService
    {
        /// <summary>
        /// ���������Զ�ע��
        /// </summary>
        Task AutoRegister();

        /// <summary>
        /// ע��һ��ʵ����������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        Task RegisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// ע��һ��ʵ����������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// ע��һ��ʵ�������񣬲�ָ������ļ�Ⱥ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        /// <param name="clusterName">��Ⱥ����</param>
        Task RegisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// ע��һ��ʵ�������񣬲�ָ������ļ�Ⱥ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        /// <param name="clusterName">��Ⱥ����</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// ע��һ��ʵ��������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="instance">ʵ������</param>
        Task RegisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// ע��һ��ʵ��������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="instance">ʵ������</param>
        Task RegisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        Task DeregisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        /// <param name="clusterName">��Ⱥ����</param>
        Task DeregisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="ip">ʵ��IP</param>
        /// <param name="port">ʵ���˿�</param>
        /// <param name="clusterName">��Ⱥ����</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="instance">ʵ������</param>
        Task DeregisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// �ӷ�����ע��һ��ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="instance">ʵ������</param>
        Task DeregisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// ��ȡ����������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        Task<IList<Instance>> GetAllInstances(string serviceName);

        /// <summary>
        /// ��ȡ����������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, bool subscribe);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// ��ȡ�����е�ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> GetAllInstances(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        Task<IList<Instance>> SelectInstances(string serviceName, bool healthy);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> SelectInstances(string serviceName, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// �ӷ����л�ȡ���õ�ʵ���б�
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="healthy">������ȡ�������Ƿǽ�����ʵ��</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<IList<Instance>> SelectInstances(string serviceName, string groupName, IList<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="subscribe">�Ƿ���</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <exception cref="ArgumentNullException">������񲻴�������׳����쳣</exception>
        Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="subscribe">�Ƿ�Ϊ�Ѷ���</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// ͨ��Ԥ����ĸ��ؾ������ѡȡһ��������ʵ��
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="subscribe">�Ƿ��Ѷ���</param>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, IList<string> clusters, bool subscribe);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="listener">����</param>
        Task Subscribe(string serviceName, Action<IEvent> listener);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="listener">����</param>
        Task Subscribe(string serviceName, string groupName, Action<IEvent> listener);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="listener">����</param>
        Task Subscribe(string serviceName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="listener">����</param>
        Task Subscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// ע������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="listener">����</param>
        void Unsubscribe(string serviceName, Action<IEvent> listener);

        /// <summary>
        /// ע������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="listener">����</param>
        void Unsubscribe(string serviceName, string groupName, Action<IEvent> listener);

        /// <summary>
        /// ע������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="listener">����</param>
        void Unsubscribe(string serviceName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// ע������
        /// </summary>
        /// <param name="serviceName">��������</param>
        /// <param name="groupName">��������</param>
        /// <param name="clusters">��Ⱥ�����б�</param>
        /// <param name="listener">����</param>
        void Unsubscribe(string serviceName, string groupName, IList<string> clusters, Action<IEvent> listener);

        /// <summary>
        /// ��ȡ���з��������
        /// </summary>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize);

        /// <summary>
        /// ��ȡ���з��������
        /// </summary>
        /// <param name="groupName">��������</param>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName);

        /// <summary>
        /// ��ȡ���з��������
        /// </summary>
        /// <param name="groupName">��������</param>
        /// <param name="selector">ѡ����</param>
        Task<ServiceList> GetServicesOfServer(int pageNo, int pageSize, string groupName, Selector selector);

        /// <summary>
        /// ��ȡ�����Ѿ����ĵķ�����Ϣ
        /// </summary>
        IList<ServiceInfo> GetSubscribeServices();

        /// <summary>
        /// ��ȡ����״̬
        /// </summary>
        Task<string> GetServerStatus();
```

## ����˵��

### �ֱ�֧��
���ǵ������Nacos����Ĺ��ϣ������ַ�����ֳ�ʱ���޷�ͨ�ŵ�������ڲ�ͨ����ʱ��ȡ�������µ����ݲ�������`cacheDir`·���µ�`failover`Ŀ¼�£�
�����������Ҫ���������л���ǰ��Ҫ����������ͨ��д���ֱ�Ŀ¼�µ�`00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00`�ļ����п��ƣ�������Ҫ
������SDKǰ��ָ��Ŀ¼�´������ļ���д���Ӧֵ��ͨ��"0"��"1"���ƣ�����"1"��������һ���������л�ȡ����ͨ���ֱ���������ݶ������������˳���    

### ��������
���ǵ�ʵ��ʹ�ø�SDK�ĳ����п��ܻ������ͬ����ͬʱע�᲻ͬ�ķ������ƻ�ע�᲻ͬ���������ҳ���IP�Ͷ˿ڲ�һ������������Ը�SDKʵ���˻���
��������+IP+Port����ķ��������Ա�֤Nacosע�����Ĳ��Ὣ����ע��ķ�����Ϊ������״̬��������Ҳ��ζ��ע����ཫ�ᵼ��һ����CPU�����ġ�    

### ʵ�����Ի�
��ʹ�ø�SDK���з���ע����������˻����Ĳ����⣬ͨ���趨ʵ���ľ���������Զ������һЩ�������ԣ�ֻ��ͨ��`Instance.MetaData`�ſ��ԣ���Ҫ
֧�ֵ��������£�
```
/// <summary>
/// �ṩʵ����������
/// </summary>
public class PreservedMetadataKeys
{
    /// <summary>
    /// ע����Դ
    /// </summary>
    public const string REGISTER_SOURCE = "preserved.register.source";

    /// <summary>
    /// ������ʱʱ��
    /// </summary>
    public const string HEART_BEAT_TIMEOUT = "preserved.heart.beat.timeout";

    /// <summary>
    /// ʵ���Ƴ���ʱʱ��
    /// </summary>
    public const string IP_DELETE_TIMEOUT = "preserved.ip.delete.timeout";

    /// <summary>
    /// �������ʱ��
    /// </summary>
    public const string HEART_BEAT_INTERVAL = "preserved.heart.beat.interval";
    }
```

### ����Nacos�����    
���ｨ��ʹ���߲ο������ĵ�����Docker������[����](https://nacos.io/zh-cn/docs/quick-start-docker.html)

## ����˵��

### ʹ��ע��
1. �ڽ��з���ע��ʱ�������Ҫ�Զ��������ռ䣬��Ҫ������Nacos����˴���һ���µ������ռ�Ȼ�󽫿ռ��Id��GUID����Ϊ��SDK��namespace��������������;    
2. 

### �ݲ�֧�ֹ���
ͨ����д�Ĺ����п��ǵ���������ֹ��ܽ������ʵ���ɾ�����˸����Ϊ�˱�֤�͹ٷ�������SDK֮������ݱ���һ�£��ⲿ����ȫ�ο��ٷ�SDK������ƿ�����
������Ҫ�����¹��ܽ��е�ɾ����ƻ�δ��֧�֣�    
* ͨ��UDPЭ��ˢ�·�����Ϣ��
* HTTPʧ�����ԣ�
* ���÷���

### δ���ƻ�֧��
1. ֧��ʧ�����ԣ�
2. ֧�ֶ༶���棬���ǵ��������ļ���
3. ֧��ͨ����·��ʵ�������ֱ�֧�֣�

### �������
1. [mockhttp](https://github.com/richardszalay/mockhttp)��ʵ��HttpClientģ�⣻
2. [moq](https://github.com/moq/moq4)��ʵ�ֻ����ӿڷ���Mock��
3. [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)��Json���л�֧�֣�
4. [Nlog](https://github.com/NLog/NLog)���ṩ��־��¼��
5. [xunit](https://github.com/xunit/xunit)����Ԫ���Կ�ܣ�

### �����汾
ֻ֧��Asp.Net Core 2.1+��Ŀ

### �޶���¼
* 19.9.18 ���NamingProxy��EventDispatcher��Ԫ����;
