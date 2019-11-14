using RichardSzalay.MockHttp;
using Sino.Nacos.Config;
using Sino.Nacos.Config.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NacosConfigUnitTest
{
    public class NacosConfigServiceTest
    {
        private ConfigParam _config;

        public NacosConfigServiceTest()
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

        ~NacosConfigServiceTest()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalConfigInfoProcessor.SnapshotPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public IConfigService CreateService(MockHttpMessageHandler handler)
        {
            return new NacosConfigService(_config, FakeHttpClientFactory.Create(handler.ToHttpClient()));
        }

        [Fact]
        public async Task GetConfigTest()
        {
            string rydataId = "rongyun";
            string zsdataId = "zgsign";
            string group = "tms";

            var mock = new MockHttpMessageHandler();
            mock.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", rydataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "test2");

            mock.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", zsdataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "value2");

            var service = CreateService(mock);

            string content = await service.GetConfig(rydataId, group);
            Assert.Equal("test2", content);

            content = await service.GetConfig(zsdataId, group);
            Assert.Equal("value2", content);

            content = await service.GetConfig("NotFound", group);
            Assert.Equal(string.Empty, content);
        }

        [Fact]
        public async Task GetConfigAndListenerTest()
        {
            string rydataId = "rongyun";
            string zsdataId = "zgsign";
            string group = "tms";
            string listenerContent = string.Empty;

            var mock = new MockHttpMessageHandler();
            mock.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", rydataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "test2");

            mock.When(HttpMethod.Get, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", zsdataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "value2");

            var service = CreateService(mock);

            string content = await service.GetConfigAndSignListener(rydataId, group, x =>
            {
                listenerContent = x;
            });
            Assert.Equal("test2", content);

            content = await service.GetConfigAndSignListener(zsdataId, group, x =>
            {
                listenerContent = x;
            });
            Assert.Equal("value2", content);

            content = await service.GetConfigAndSignListener("NotFound", group, x =>
            {
                listenerContent = x;
            });
            Assert.Equal(string.Empty, content);
        }

        [Fact]
        public async Task AddListenerTest()
        {
            string rydataId = "rongyun";
            string group = "tms";
            string listenerContent = string.Empty;

            var mock = new MockHttpMessageHandler();

            var service = CreateService(mock);

            Action<string> listener = x =>
            {
                listenerContent = x;
            };

            await service.AddListener(rydataId, group, listener);

            service.RemoveListener(rydataId, group, listener);
        }

        [Fact]
        public async Task PublishConfigTest()
        {
            string rydataId = "rongyun";
            string zsdataId = "zgsign";
            string group = "tms";

            var mock = new MockHttpMessageHandler();
            var ryRequest1 = mock.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithFormData("dataId", rydataId)
                .WithFormData("group", group)
                .WithFormData("tenant", _config.Namespace)
                .WithFormData("content", "test1")
                .Respond("application/json", "true");

            var ryRequest2 = mock.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithFormData("dataId", rydataId)
                .WithFormData("group", group)
                .WithFormData("tenant", _config.Namespace)
                .WithFormData("content", "test2")
                .Respond("application/json", "true");

            var zsRequest1 = mock.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithFormData("dataId", zsdataId)
                .WithFormData("group", group)
                .WithFormData("tenant", _config.Namespace)
                .WithFormData("content", "value1")
                .Respond("application/json", "true");

            var zsRequest2 = mock.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithFormData("dataId", zsdataId)
                .WithFormData("group", group)
                .WithFormData("tenant", _config.Namespace)
                .WithFormData("content", "value2")
                .Respond("application/json", "true");

            var service = CreateService(mock);

            bool result = await service.PublishConfig(rydataId, group, "test1");
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest1));

            result = await service.PublishConfig(rydataId, group, "test1");
            Assert.True(result);
            Assert.Equal(2, mock.GetMatchCount(ryRequest1));

            result = await service.PublishConfig(rydataId, group, "test2");
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest2));

            result = await service.PublishConfig(zsdataId, group, "value1");
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(zsRequest1));

            result = await service.PublishConfig(zsdataId, group, "value2");
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(zsRequest2));
        }

        [Fact]
        public async Task PublishConfigFailedTest()
        {
            string rydataId = "rongyun";
            string group = "tms";

            var mock = new MockHttpMessageHandler();

            var ryRequest1 = mock.When(HttpMethod.Post, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithFormData("dataId", rydataId)
                .WithFormData("group", group)
                .WithFormData("tenant", _config.Namespace)
                .Respond("application/json", "false");

            var service = CreateService(mock);

            bool result = await service.PublishConfig(rydataId, group, "test1");
            Assert.False(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest1));

            result = await service.PublishConfig("NotFound", group, "test2");
            Assert.False(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest1));
        }

        [Fact]
        public async Task RemoveConfigTest()
        {
            string rydataId = "rongyun";
            string zsdataId = "zgsign";
            string group = "tms";

            var mock = new MockHttpMessageHandler();
            var ryRequest1 = mock.When(HttpMethod.Delete, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", rydataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "true");

            var zsRequest1 = mock.When(HttpMethod.Delete, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", zsdataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "true");

            var service = CreateService(mock);

            bool result = await service.RemoveConfig(rydataId, group);
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest1));

            result = await service.RemoveConfig(rydataId, group);
            Assert.True(result);
            Assert.Equal(2, mock.GetMatchCount(ryRequest1));

            result = await service.RemoveConfig(zsdataId, group);
            Assert.True(result);
            Assert.Equal(1, mock.GetMatchCount(zsRequest1));

            result = await service.RemoveConfig(zsdataId, group);
            Assert.True(result);
            Assert.Equal(2, mock.GetMatchCount(zsRequest1));
        }

        [Fact]
        public async Task RemoveConfigFailedTest()
        {
            string rydataId = "rongyun";
            string group = "tms";

            var mock = new MockHttpMessageHandler();

            var ryRequest = mock.When(HttpMethod.Delete, _config.ServerAddr[0] + Constants.CONFIG_CONTROLLER_PATH)
                .WithQueryString("dataId", rydataId)
                .WithQueryString("group", group)
                .WithQueryString("tenant", _config.Namespace)
                .Respond("application/json", "false");

            var service = CreateService(mock);

            bool result = await service.RemoveConfig(rydataId, group);
            Assert.False(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest));

            result = await service.RemoveConfig("NotFound", group);
            Assert.False(result);
            Assert.Equal(1, mock.GetMatchCount(ryRequest));
        }
    }
}
