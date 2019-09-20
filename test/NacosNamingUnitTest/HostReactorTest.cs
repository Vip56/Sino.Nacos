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
using Xunit;
using System.Threading;

namespace NacosNamingUnitTest
{
    public class HostReactorTest
    {
        private NamingConfig _config;
        private ServiceInfo _orderServiceInfo;
        private ServiceInfo _inquiryServiceInfo;

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
        }

        private NamingProxy MockNamingProxy()
        {
            var mockHttp = new MockHttpMessageHandler();

            Func<HttpRequestMessage, HttpContent> orderHandler = x =>
            {
                return new StringContent(JsonConvert.SerializeObject(_orderServiceInfo), Encoding.UTF8, "application/json");
            };

            Func<HttpRequestMessage, HttpContent> inquiryHandler = x =>
            {
                return new StringContent(JsonConvert.SerializeObject(_inquiryServiceInfo), Encoding.UTF8, "application/json");
            };

            // QueryList
            mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/list")
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _orderServiceInfo.Name)
                .WithQueryString(NamingProxy.CLUSTERS_KEY, _orderServiceInfo.Clusters)
                .WithQueryString(NamingProxy.HEALTHY_ONLY, "False")
                .Respond(orderHandler);

            mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/list")
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _inquiryServiceInfo.Name)
                .WithQueryString(NamingProxy.CLUSTERS_KEY, _inquiryServiceInfo.Clusters)
                .WithQueryString(NamingProxy.HEALTHY_ONLY, "False")
                .Respond(inquiryHandler);

            return new NamingProxy(_config, new FastHttp(FakeHttpClientFactory.Create(mockHttp.ToHttpClient()), _config));
        }

        [Fact]
        public async Task GetServiceInfoTest()
        {
            var orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);

            Assert.NotNull(orderInfo);
            Assert.Equal(orderInfo.Name, _orderServiceInfo.Name);
            Assert.Equal(orderInfo.GroupName, _orderServiceInfo.GroupName);
            Assert.Equal(orderInfo.Clusters, _orderServiceInfo.Clusters);
            Assert.Equal(orderInfo.Hosts.Count, _orderServiceInfo.Hosts.Count);
            Assert.Equal(orderInfo.Hosts.First().InstanceId, _orderServiceInfo.Hosts.First().InstanceId);
            Assert.Equal(orderInfo.Hosts.First().Ip, _orderServiceInfo.Hosts.First().Ip);
            Assert.Equal(orderInfo.Hosts.First().Metadata.Count, _orderServiceInfo.Hosts.First().Metadata.Count);
            Assert.Equal(orderInfo.Hosts.First().Port, _orderServiceInfo.Hosts.First().Port);
            Assert.Equal(orderInfo.Hosts.First().ServiceName, _orderServiceInfo.Hosts.First().ServiceName);
            Assert.Equal(orderInfo.LastRefTime, _orderServiceInfo.LastRefTime);

            orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);

            Assert.NotNull(orderInfo);
        }

        [Fact]
        public async Task GetServiceInfoWithAutoFlushTest()
        {
            HostReactor.DEFAULT_DELAY = 500;

            var orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);

            Assert.NotNull(orderInfo);

            _orderServiceInfo.Hosts.First().Ip = "192.168.2.50";
            _orderServiceInfo.Hosts.First().Port = 5500;
            _orderServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            Thread.Sleep(1100);

            orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);

            Assert.NotNull(orderInfo);
            Assert.Equal(orderInfo.Hosts.First().Ip, _orderServiceInfo.Hosts.First().Ip);
            Assert.Equal(orderInfo.Hosts.First().Port, _orderServiceInfo.Hosts.First().Port);
        }

        [Fact]
        public async Task GetTwoServiceInfoTest()
        {
            var orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);
            var inquiryInfo = await _hostReactor.GetServiceInfo(_inquiryServiceInfo.Name, _orderServiceInfo.Clusters);

            Assert.NotNull(orderInfo);
            Assert.NotNull(inquiryInfo);
            Assert.Equal(inquiryInfo.Name, _inquiryServiceInfo.Name);
            Assert.Equal(inquiryInfo.GroupName, _inquiryServiceInfo.GroupName);
            Assert.Equal(inquiryInfo.Clusters, _inquiryServiceInfo.Clusters);
            Assert.Equal(inquiryInfo.Hosts.Count, _inquiryServiceInfo.Hosts.Count);
            Assert.Equal(inquiryInfo.Hosts.First().InstanceId, _inquiryServiceInfo.Hosts.First().InstanceId);
            Assert.Equal(inquiryInfo.Hosts.First().Ip, _inquiryServiceInfo.Hosts.First().Ip);
            Assert.Equal(inquiryInfo.Hosts.First().Metadata.Count, _inquiryServiceInfo.Hosts.First().Metadata.Count);
            Assert.Equal(inquiryInfo.Hosts.First().Port, _inquiryServiceInfo.Hosts.First().Port);
            Assert.Equal(inquiryInfo.Hosts.First().ServiceName, _inquiryServiceInfo.Hosts.First().ServiceName);
            Assert.Equal(inquiryInfo.LastRefTime, _inquiryServiceInfo.LastRefTime);

            _inquiryServiceInfo.Hosts.First().Ip = "192.168.1.1";
            _inquiryServiceInfo.Hosts.First().Port = 5300;

            _orderServiceInfo.Hosts.First().Ip = "192.168.2.1";
            _orderServiceInfo.Hosts.First().Port = 5200;

            _inquiryServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();
            _orderServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            Thread.Sleep(1100);

            orderInfo = await _hostReactor.GetServiceInfo(_orderServiceInfo.Name, _orderServiceInfo.Clusters);
            inquiryInfo = await _hostReactor.GetServiceInfo(_inquiryServiceInfo.Name, _inquiryServiceInfo.Clusters);

            Assert.NotNull(inquiryInfo);
            Assert.NotNull(orderInfo);
            Assert.Equal(orderInfo.Hosts.First().Ip, _orderServiceInfo.Hosts.First().Ip);
            Assert.Equal(orderInfo.Hosts.First().Port, _orderServiceInfo.Hosts.First().Port);
            Assert.Equal(inquiryInfo.Hosts.First().Ip, _inquiryServiceInfo.Hosts.First().Ip);
            Assert.Equal(inquiryInfo.Hosts.First().Port, _inquiryServiceInfo.Hosts.First().Port);
        }
    }
}
