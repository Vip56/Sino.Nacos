using RichardSzalay.MockHttp;
using Sino.Nacos.Config;
using System;
using Xunit;

namespace NacosConfigUnitTest
{
    public class ServerHttpAgentTest
    {
        private ConfigParam _config;

        private MockHttpMessageHandler _mockHttp;

        public ServerHttpAgentTest()
        {
            _config = new ConfigParam
            {

            };

            _mockHttp = new MockHttpMessageHandler();


        }

        [Fact]
        public void Test1()
        {

        }
    }
}
