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
        public async Task DeregisterService()
        {

        }
    }
}
