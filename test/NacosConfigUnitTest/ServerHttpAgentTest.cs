using RichardSzalay.MockHttp;
using Sino.Nacos.Config;
using Sino.Nacos.Config.Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace NacosConfigUnitTest
{
    public class ServerHttpAgentTest
    {
        private ConfigParam _config;

        private MockHttpMessageHandler _mockHttp;
        private MockedRequest _getMockedRequest;
        private MockedRequest _postMockedRequest;
        private MockedRequest _deleteMockedRequest;
        private MockedRequest _endpointMockedReqeust;

        public ServerHttpAgentTest()
        {
            _config = new ConfigParam
            {
                ServerAddr = new List<string>() { "http://test:8338" },
                EndPoint = "http://server:8338",
                Namespace = "default"
            };

            _mockHttp = new MockHttpMessageHandler();

            _getMockedRequest = _mockHttp.When(HttpMethod.Get, _config.ServerAddr[0] + "/g")
                .WithQueryString("k1", "v1")
                .WithHeaders("Client-Version", ServerHttpAgent.VERSION)
                .WithHeaders("User-Agent", ServerHttpAgent.VERSION)
                .WithHeaders("Request-Module", "Naming")
                .WithHeaders("h1", "v1")
                .Respond("application/json", "ok");

            _postMockedRequest = _mockHttp.When(HttpMethod.Post, _config.ServerAddr[0] + "/p")
                .WithQueryString("k2", "v2")
                .WithHeaders("Client-Version", ServerHttpAgent.VERSION)
                .WithHeaders("User-Agent", ServerHttpAgent.VERSION)
                .WithHeaders("Request-Module", "Naming")
                .WithHeaders("h2", "v2")
                .Respond("application/json", "ok");

            _deleteMockedRequest = _mockHttp.When(HttpMethod.Delete, _config.ServerAddr[0] + "/d")
                .WithQueryString("k3", "v3")
                .WithHeaders("Client-Version", ServerHttpAgent.VERSION)
                .WithHeaders("User-Agent", ServerHttpAgent.VERSION)
                .WithHeaders("Request-Module", "Naming")
                .WithHeaders("h3", "v3")
                .Respond("application/json", "ok");

            _endpointMockedReqeust = _mockHttp.When(HttpMethod.Get, _config.EndPoint + "/nacos/serverlist")
                .Respond("plain/text", "http://test:8338\r\n");
        }

        private IHttpAgent CreateAgent()
        {
            return new ServerHttpAgent(_config, new FastHttp(FakeHttpClientFactory.Create(_mockHttp.ToHttpClient()), _config));
        }

        [Fact]
        public async Task GetTest()
        {
            var agent = CreateAgent();

            var request = await agent.Get("/g",
                new Dictionary<string, string>
                {
                    { "h1","v1" }
                },
                new Dictionary<string, string>
                {
                    { "k1","v1" }
                });

            Assert.Equal("ok", request);
            Assert.Equal(1, _mockHttp.GetMatchCount(_getMockedRequest));
        }

        [Fact]
        public async Task PostTest()
        {
            var agent = CreateAgent();

            var request = await agent.Post("/p", 
                new Dictionary<string, string>
                {
                    { "h2", "v2" }
                },
                new Dictionary<string, string>
                {
                    { "k2", "v2" }
                });

            Assert.Equal("ok", request);
            Assert.Equal(1, _mockHttp.GetMatchCount(_postMockedRequest));
        }

        [Fact]
        public async Task DeleteTest()
        {
            var agent = CreateAgent();

            var request = await agent.Delete("/d",
                new Dictionary<string, string>
                {
                    { "h3", "v3" }
                },
                new Dictionary<string, string>
                {
                    { "k3", "v3" }
                });

            Assert.Equal("ok", request);
            Assert.Equal(1, _mockHttp.GetMatchCount(_deleteMockedRequest));
        }

        [Fact]
        public async Task GetServerListFromEndPointTest()
        {
            _config.ServerAddr.Clear();

            var agent = CreateAgent();

            var request = await agent.Get("/g",
                new Dictionary<string, string>
                {
                    { "h1","v1" }
                },
                new Dictionary<string, string>
                {
                    { "k1","v1" }
                });

            Assert.Equal("ok", request);
            Assert.Equal(1, _mockHttp.GetMatchCount(_getMockedRequest));
            Assert.Equal(1, _mockHttp.GetMatchCount(_endpointMockedReqeust));
        }
    }
}
