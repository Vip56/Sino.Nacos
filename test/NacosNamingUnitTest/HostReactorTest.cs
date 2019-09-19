using RichardSzalay.MockHttp;
using Sino.Nacos.Naming;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Sino.Nacos.Naming.Utils;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Sino.Nacos.Naming.Core;
using System.IO;
using System.Threading.Tasks;

namespace NacosNamingUnitTest
{
    public class HostReactorTest
    {
        private NamingConfig _config;
        private ServiceInfo _orderServiceInfo;
        private ServiceInfo _inquiryServiceInfo;
        private Service _service;
        private BeatInfo _beatInfo;

        private HostReactor _hostReactor;

        public HostReactorTest()
        {
            _config = new NamingConfig
            {
                EndPoint = "http://localhost:8848",
                ServerAddr = new List<string>() { "http://localhost:8848" },
                Namespace = "vip56"
            };
            MockData();
            var namingProxy = MockNamingProxy();
            var eventDispatcher = new EventDispatcher();
            _hostReactor = new HostReactor(eventDispatcher, namingProxy, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nacos"));
        }

        private void MockData()
        {
            _orderServiceInfo = new ServiceInfo();
            _orderServiceInfo.Name = "tms_order_v1";
            _orderServiceInfo.GroupName = "tms";
            _orderServiceInfo.Clusters = "test";
            var orderInstance = new Instance()
            {
                InstanceId = "1",
                Ip = "192.168.1.50",
                Port = 5000,
                Weight = 1,
                ClusterName = "test",
                ServiceName = "tms_order_v1"
            };
            orderInstance.Metadata.Add("k1", "v1");
            _orderServiceInfo.Hosts.Add(orderInstance);
            _orderServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            _inquiryServiceInfo = new ServiceInfo();
            _inquiryServiceInfo.Name = "tms_inquiry_v1";
            _inquiryServiceInfo.GroupName = "tms";
            _inquiryServiceInfo.Clusters = "test";
            var inquiryInstance = new Instance()
            {
                InstanceId = "2",
                Ip = "192.168.1.51",
                Port = 5000,
                Weight = 1,
                ClusterName = "test",
                ServiceName = "tms_order_v1"
            };
            inquiryInstance.Metadata.Add("k2", "v2");
            _inquiryServiceInfo.Hosts.Add(inquiryInstance);
            _inquiryServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            _service = new Service("tms_order_v1");
            _service.GroupName = "test";
            _service.ProtectThreshold = 1;
            _service.AppName = "testhost";
            _service.Metadata.Add("k1", "v1");

            _beatInfo = new BeatInfo();
            _beatInfo.Port = 5000;
            _beatInfo.Ip = "192.168.1.101";
            _beatInfo.Weight = 1;
            _beatInfo.ServiceName = "tms_order_v1";
            _beatInfo.Cluster = "tms";
            _beatInfo.MetaData.Add("k1", "v1");
            _beatInfo.Scheduled = true;
            _beatInfo.PerId = 1000;
            _beatInfo.Stopped = false;
        }

        private NamingProxy MockNamingProxy()
        {
            var mockHttp = new MockHttpMessageHandler();

            // RegisterService
            mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _orderServiceInfo.Name)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, _orderServiceInfo.GroupName)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, _orderServiceInfo.Clusters)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, _orderServiceInfo.Hosts.First().Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, _orderServiceInfo.Hosts.First().Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_WEIGHT_KEY, _orderServiceInfo.Hosts.First().Weight.ToString())
                .WithQueryString(NamingProxy.SERVICE_ENABLE_KEY, _orderServiceInfo.Hosts.First().Enable.ToString())
                .WithQueryString(NamingProxy.SERVICE_HEALTHY_KEY, _orderServiceInfo.Hosts.First().Healthy.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, _orderServiceInfo.Hosts.First().Ephemeral.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(_orderServiceInfo.Hosts.First().Metadata))
                .Respond("application/json", "ok");

            // DeregisterService
            mockHttp.When(HttpMethod.Delete, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_ENABLE_KEY, _orderServiceInfo.Name)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, _orderServiceInfo.Clusters)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, _orderServiceInfo.Hosts.First().Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, _orderServiceInfo.Hosts.First().Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, _orderServiceInfo.Hosts.First().Ephemeral.ToString())
                .Respond("application/json", "ok");

            // UpdateInstance
            mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_INSTANCE)
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _orderServiceInfo.Name)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, _orderServiceInfo.GroupName)
                .WithQueryString(NamingProxy.CLUSTER_NAME_KEY, _orderServiceInfo.Clusters)
                .WithQueryString(NamingProxy.SERVICE_IP_KEY, _orderServiceInfo.Hosts.First().Ip)
                .WithQueryString(NamingProxy.SERVICE_PORT_KEY, _orderServiceInfo.Hosts.First().Port.ToString())
                .WithQueryString(NamingProxy.SERVICE_WEIGHT_KEY, _orderServiceInfo.Hosts.First().Weight.ToString())
                .WithQueryString(NamingProxy.SERVICE_ENABLE_KEY, _orderServiceInfo.Hosts.First().Enable.ToString())
                .WithQueryString(NamingProxy.SERVICE_EPHEMERAL_KEY, _orderServiceInfo.Hosts.First().Ephemeral.ToString())
                .WithQueryString(NamingProxy.SERVICE_METADATA_KEY, JsonConvert.SerializeObject(_orderServiceInfo.Hosts.First().Metadata))
                .Respond("application/json", "ok");

            // QueryService
            mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/list")
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _service.Name)
                .WithQueryString(NamingProxy.CLUSTERS_KEY, "tms")
                .WithQueryString(NamingProxy.HEALTHY_ONLY, "True")
                .Respond("application/json", JsonConvert.SerializeObject(_service));

            // SendBeat
            mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/beat")
                .WithQueryString(NamingProxy.BEAT_KEY, _beatInfo.ToString())
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _beatInfo.ServiceName)
                .Respond("application/json", "ok");

            // ServerHealthy
            ServiceMetrics mockServiceMetrics = new ServiceMetrics();
            mockServiceMetrics.ServiceCount = 336;
            mockServiceMetrics.Load = 0.09f;
            mockServiceMetrics.Mem = 0.46210432f;
            mockServiceMetrics.ResponsibleServiceCount = 98;
            mockServiceMetrics.InstanceCount = 4;
            mockServiceMetrics.Cpu = 0.010242796f;
            mockServiceMetrics.Status = "UP";
            mockServiceMetrics.ResponsibleInstanceCount = 0;

            var request = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/operator/metrics")
                .Respond("application/json", JsonConvert.SerializeObject(mockServiceMetrics));

            // GetServiceList
            ServiceList mockServiceList = new ServiceList();
            mockServiceList.Count = 336;
            mockServiceList.Doms.Add("tms_order_v1");
            mockServiceList.Doms.Add("tms_order_v2");

            mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/service/list")
                .WithQueryString(NamingProxy.PAGE_NO_KEY, "1")
                .WithQueryString(NamingProxy.PAGE_SIZE_KEY, "2")
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.GROUP_NAME_KEY, "test")
                .Respond("application/json", JsonConvert.SerializeObject(mockServiceList));

            // GetServerListFromEndpoint
            string[] servers = new string[] { "http://192.168.1.1:8848", "http://192.168.1.2:8848" };

            mockHttp.When(HttpMethod.Get, _config.EndPoint + "/nacos/serverlist")
                .Respond("application/json", string.Join("\r\n", servers));

            return new NamingProxy(_config, new FastHttp(FakeHttpClientFactory.Create(mockHttp.ToHttpClient()), _config));
        }

        public async Task  
    }
}
