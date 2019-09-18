using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Sino.Nacos.Naming;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Sino.Nacos.Naming.Utils;

namespace NacosNamingUnitTest
{
    public class NamingProxyTest
    {
        NamingConfig _config;

        public NamingProxyTest()
        {
            _config = new NamingConfig
            {
                EndPoint = "http://localhost:8848",
                ServerAddr = new List<string>() { "http://localhost:8848" },
                Namespace = "vip56"
            };
        }

        private NamingProxy CreateProxy(MockHttpMessageHandler handler)
        {
            return new NamingProxy(_config, new FastHttp(FakeHttpClientFactory.Create(handler.ToHttpClient()), _config));
        }

        [Fact]
        public async Task RegisterServiceTest()
        {
            string serviceName = "tms_order_v1";
            string groupName = "test";
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, groupName)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, instance.ClusterName)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, instance.Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, instance.Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_WEIGHT_KEY, instance.Weight.ToString())
                .WithQueryString(NamingProxy.SERVICE_ENABLE_KEY, instance.Enable.ToString())
                .WithQueryString(NamingProxy.SERVICE_HEALTHY_KEY, instance.Healthy.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, instance.Ephemeral.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(instance.Metadata))
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);

            var response = await proxy.RegisterService(serviceName, groupName, instance);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal("ok", response);
        }

        [Fact]
        public async Task DeregisterServiceTest()
        {
            string serviceName = "tms_order_v1";
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Ephemeral = true;

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Delete, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, instance.ClusterName)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, instance.Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, instance.Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, instance.Ephemeral.ToString())
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);

            var response = await proxy.DeregisterService(serviceName, instance);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal("ok", response);
        }

        [Fact]
        public async Task UpdateInstanceTest()
        {
            string serviceName = "tms_order_v1";
            string groupName = "test";
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 2;
            instance.Enable = false;
            instance.Ephemeral = false;
            instance.Metadata.Add("t1", "v1");
            instance.Metadata.Add("t2", "v2");

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, groupName)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, instance.ClusterName)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, instance.Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, instance.Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_WEIGHT_KEY, instance.Weight.ToString())
                .WithQueryString(NamingProxy.SERVICE_ENABLE_KEY, instance.Enable.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, instance.Ephemeral.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(instance.Metadata))
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.UpdateInstance(serviceName, groupName, instance);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal("ok", response);
        }

        [Fact]
        public async Task QueryServiceTest()
        {
            string serviceName = "tms_order_v1";
            string groupName = "test";

            Service mockResponse = new Service(serviceName);
            mockResponse.GroupName = groupName;
            mockResponse.ProtectThreshold = 1;
            mockResponse.AppName = "testhost";
            mockResponse.Metadata.Add("k1", "v1");
            mockResponse.Metadata.Add("k2", "v2");

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_SERVICE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, groupName)
                .Respond("application/json", JsonConvert.SerializeObject(mockResponse));

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.QueryService(serviceName, groupName);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(mockResponse.GroupName, response.GroupName);
            Assert.Equal(mockResponse.ProtectThreshold, response.ProtectThreshold);
            Assert.Equal(mockResponse.AppName, response.AppName);
            Assert.Equal(mockResponse.Metadata.Count, response.Metadata.Count);
            Assert.Equal(mockResponse.Metadata["k1"], response.Metadata["k1"]);
            Assert.Equal(mockResponse.Metadata["k2"], response.Metadata["k2"]);
        }

        [Fact]
        public async Task CreateServiceTest()
        {
            Service service = new Service("tms_order_v1");
            service.GroupName = "test";
            service.ProtectThreshold = 0;
            service.Metadata.Add("k1", "v1");
            service.Metadata.Add("k2", "v2");

            Selector selector = new Selector();
            selector.Type = "tms-order";

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_SERVICE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, service.Name)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, service.GroupName)
                .WithQueryString(NamingProxy.PROTECT_THRESHOLD_KEY, service.ProtectThreshold.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(service.Metadata))
                .WithQueryString(NamingProxy.SELECTOR_KEY, JsonConvert.SerializeObject(selector))
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.CreateService(service, selector);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal("ok", response);
        }

        [Fact]
        public async Task DeleteServiceTest()
        {
            string serviceName = "tms_order_v1";
            string groupName = "test";

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Delete, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_SERVICE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, groupName)
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.DeleteService(serviceName, groupName);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.True(response);
        }

        [Fact]
        public async Task UpdateServiceTest()
        {
            Service service = new Service("tms_order_v2");
            service.GroupName = "test";
            service.ProtectThreshold = 1;
            service.Metadata.Add("k3", "v3");
            service.Metadata.Add("k4", "v4");

            Selector selector = new Selector();
            selector.Type = "tms";

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_SERVICE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, service.Name)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, service.GroupName)
                .WithQueryString(NamingProxy.PROTECT_THRESHOLD_KEY, service.ProtectThreshold.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(service.Metadata))
                .WithQueryString(NamingProxy.SELECTOR_KEY, JsonConvert.SerializeObject(selector))
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.UpdateService(service, selector);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal("ok", response);
        }

        [Fact]
        public async Task QueryListTest()
        {
            string serviceName = "tms_order_v1";
            string clusters = "tms";

            Instance instance = new Instance();
            instance.ClusterName = clusters;
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            ServiceInfo serviceInfo = new ServiceInfo();
            serviceInfo.Name = serviceName;
            serviceInfo.GroupName = "test";
            serviceInfo.Clusters = clusters;
            serviceInfo.CacheMillis = 1500;
            serviceInfo.LastRefTime = DateTime.Now.GetTimeStamp();
            serviceInfo.CheckSum = "3bbcf6dd1175203a8afdade0e77a27cd1528787794594";
            serviceInfo.AllIPs = false;
            serviceInfo.Hosts.Add(instance);

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/list")
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, serviceName)
                .WithQueryString(NamingProxy.CLUSTERS_KEY, clusters)
                .WithQueryString(NamingProxy.HEALTHY_ONLY, "True")
                .WithQueryString("app", "testhost")
                .Respond("application/json", JsonConvert.SerializeObject(serviceInfo));

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.QueryList(serviceName, clusters, 0, true);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(serviceInfo.Name, response.Name);
            Assert.Equal(serviceInfo.GroupName, response.GroupName);
            Assert.Equal(serviceInfo.Clusters, response.Clusters);
            Assert.Equal(serviceInfo.CacheMillis, response.CacheMillis);
            Assert.Equal(serviceInfo.LastRefTime, response.LastRefTime);
            Assert.Equal(serviceInfo.CheckSum, response.CheckSum);
            Assert.Equal(serviceInfo.AllIPs, response.AllIPs);
            Assert.Equal(serviceInfo.Hosts.Count, response.Hosts.Count);
            Assert.Equal(serviceInfo.Hosts[0].ClusterName, response.Hosts[0].ClusterName);
            Assert.Equal(serviceInfo.Hosts[0].Ip, response.Hosts[0].Ip);
            Assert.Equal(serviceInfo.Hosts[0].Port, response.Hosts[0].Port);
            Assert.Equal(serviceInfo.Hosts[0].Weight, response.Hosts[0].Weight);
            Assert.Equal(serviceInfo.Hosts[0].Enable, response.Hosts[0].Enable);
            Assert.Equal(serviceInfo.Hosts[0].Healthy, response.Hosts[0].Healthy);
            Assert.Equal(serviceInfo.Hosts[0].Ephemeral, response.Hosts[0].Ephemeral);
            Assert.Equal(serviceInfo.Hosts[0].Metadata.Count, response.Hosts[0].Metadata.Count);
        }

        [Fact]
        public async Task SendBeatTest()
        {
            BeatInfo beatInfo = new BeatInfo();
            beatInfo.Port = 5000;
            beatInfo.Ip = "192.168.1.101";
            beatInfo.Weight = 1;
            beatInfo.ServiceName = "tms_order_v1";
            beatInfo.Cluster = "tms";
            beatInfo.MetaData.Add("k1", "v1");
            beatInfo.Scheduled = true;
            beatInfo.PerId = 1000;
            beatInfo.Stopped = false;

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/beat")
                .WithQueryString(NamingProxy.BEAT_KEY, beatInfo.ToString())
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, beatInfo.ServiceName)
                .Respond("application/json", "ok");

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.SendBeat(beatInfo);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(0, response);
        }

        [Fact]
        public async Task ServerHealthyTest()
        {
            ServiceMetrics mockServiceMetrics = new ServiceMetrics();
            mockServiceMetrics.ServiceCount = 336;
            mockServiceMetrics.Load = 0.09f;
            mockServiceMetrics.Mem = 0.46210432f;
            mockServiceMetrics.ResponsibleServiceCount = 98;
            mockServiceMetrics.InstanceCount = 4;
            mockServiceMetrics.Cpu = 0.010242796f;
            mockServiceMetrics.Status = "UP";
            mockServiceMetrics.ResponsibleInstanceCount = 0;

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/operator/metrics")
                .Respond("application/json", JsonConvert.SerializeObject(mockServiceMetrics));

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.ServerHealthy();

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.True(response);
        }

        [Fact]
        public async Task GetServiceListTest()
        {
            int pageNo = 1;
            int pageSize = 2;
            string groupName = "test";

            ServiceList mockServiceList = new ServiceList();
            mockServiceList.Count = 336;
            mockServiceList.Doms.Add("tms_order_v1");
            mockServiceList.Doms.Add("tms_order_v2");

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/service/list")
                .WithQueryString(NamingProxy.PAGE_NO_KEY, pageNo.ToString())
                .WithQueryString(NamingProxy.PAGE_SIZE_KEY, pageSize.ToString())
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, groupName)
                .Respond("application/json", JsonConvert.SerializeObject(mockServiceList));

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.GetServiceList(pageNo, pageSize, groupName, null);

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(mockServiceList.Count, response.Count);
            Assert.Equal(mockServiceList.Doms[0], response.Doms[0]);
            Assert.Equal(mockServiceList.Doms[1], response.Doms[1]);
        }

        [Fact]
        public async Task GeterverListFromEndpointTest()
        {
            string[] servers = new string[] { "http://192.168.1.1:8848", "http://192.168.1.2:8848" };

            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(HttpMethod.Get, _config.EndPoint + "/nacos/serverlist")
                .Respond("application/json", string.Join("\r\n", servers));

            var proxy = CreateProxy(mockHttp);
            var response = await proxy.GetServerListFromEndpoint();

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(servers.Length, response.Count);
        }
    }
}
