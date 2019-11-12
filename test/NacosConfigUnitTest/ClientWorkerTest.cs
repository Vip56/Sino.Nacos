using RichardSzalay.MockHttp;
using Sino.Nacos.Config;
using Sino.Nacos.Config.Core;
using Sino.Nacos.Config.Filter;
using Sino.Nacos.Config.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NacosConfigUnitTest
{
    public class ClientWorkerTest
    {
        private ConfigParam _config;
        private IHttpAgent _httpAgent;
        private ConfigFilterChainManager _filter;
        private LocalConfigInfoProcessor _localConfigInfoProcessor;

        public ClientWorkerTest()
        {
            _config = new ConfigParam
            {
                ServerAddr = new List<string>
                {
                    "http://localhost:8443"
                },
                LocalFileRoot = AppDomain.CurrentDomain.BaseDirectory,
                Namespace = "dev"
            };
        }

        ~ClientWorkerTest()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalConfigInfoProcessor.SnapshotPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public ClientWorker CreateWorker(MockHttpMessageHandler handler)
        {
            _httpAgent = new ServerHttpAgent(_config, new FastHttp(FakeHttpClientFactory.Create(handler.ToHttpClient()), _config));

            _filter = new ConfigFilterChainManager();
            _localConfigInfoProcessor = new LocalConfigInfoProcessor(_config.LocalFileRoot);

            return new ClientWorker(_config, _httpAgent, _filter, _localConfigInfoProcessor);
        }

        [Fact]
        public void ListenerAddAndFireTest()
        {
            var mockHttp = new MockHttpMessageHandler();

            string dataId = "rongyun";
            string group = "tms";
            string content = string.Empty;
            int fireCount = 0;

            var getRequest = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", dataId)
                .WithQueryString("group", group)
                .Respond("application/json", "test");

            var postRequest = mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH + "/listener")
                .WithHeaders("Long-Pulling-Timeout", _config.ConfigLongPollTimeout.ToString())
                .WithFormData(Constants.PROBE_MODIFY_REQUEST, $"{dataId}\u0002{group}\u0002\u0002{Constants.DEFAULT_TENANT_ID}\u0001")
                .Respond("application/json", $"{dataId}\u0002{group}\u0002\u0001");

            var client = CreateWorker(mockHttp);

            Action<string> listener = x =>
            {
                fireCount++;
                content = x;
            };

            client.AddListeners(dataId, group, listener);

            Thread.Sleep(100);

            Assert.Equal(1, fireCount);
            Assert.Equal("test", content);
        }

        [Fact]
        public void ListenerAddAndRemoveTest()
        {
            var mockHttp = new MockHttpMessageHandler();

            string dataId = "rongyun";
            string group = "tms";
            string content = string.Empty;
            int fireCount = 0;

            var getRequest = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", dataId)
                .WithQueryString("group", group)
                .Respond("application/json", "test");

            var postRequest = mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH + "/listener")
                .WithHeaders("Long-Pulling-Timeout", _config.ConfigLongPollTimeout.ToString())
                .WithFormData(Constants.PROBE_MODIFY_REQUEST, $"{dataId}\u0002{group}\u0002\u0002{Constants.DEFAULT_TENANT_ID}\u0001")
                .Respond("application/json", $"{dataId}\u0002{group}\u0002\u0001");

            var client = CreateWorker(mockHttp);

            Action<string> listener = x =>
            {
                fireCount++;
                content = x;
            };

            Action<string> listener2 = x =>
            {
                fireCount++;
                content = x;
            };

            client.AddListeners(dataId, group, listener);
            client.AddListeners(dataId, group, listener2);
            client.RemoveListener(dataId, group, listener);

            Thread.Sleep(100);

            Assert.Equal(1, fireCount);
            Assert.Equal("test", content);
        }

        [Fact]
        public async Task TenantListenerAddAndFireTest()
        {
            var mockHttp = new MockHttpMessageHandler();

            string dataId = "rongyun";
            string group = "tms";
            string content = string.Empty;
            int fireCount = 0;

            var getRequest = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", dataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "test");

            var client = CreateWorker(mockHttp);

            Action<string> listener = x =>
            {
                fireCount++;
                content = x;
            };

            Action<string> listener2 = x =>
            {
                fireCount++;
                content = x;
            };

            await client.AddTenantListeners(dataId, group, listener);
            await client.AddTenantListenersWithContent(dataId, group, "test2", listener2);

            Thread.Sleep(100);

            Assert.Equal(1, fireCount);
            Assert.Equal("test2", content);

            client.RemoveTenantListener(dataId, group, listener);
        }

        [Fact]
        public async Task GetServerConfigTest()
        {
            var mockHttp = new MockHttpMessageHandler();

            string dataId = "rongyun";
            string group = "tms";

            var getRequest = mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", dataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "test");

            var client = CreateWorker(mockHttp);

            var content = await client.GetServerConfig(dataId, group, _config.Namespace);

            Assert.Equal("test", content);
        }
    }
}
