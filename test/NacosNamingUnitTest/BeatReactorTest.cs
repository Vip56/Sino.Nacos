using RichardSzalay.MockHttp;
using Sino.Nacos.Naming;
using Sino.Nacos.Naming.Beat;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace NacosNamingUnitTest
{
    public class BeatReactorTest
    {
        private NamingConfig _config;

        private MockHttpMessageHandler _mockHttp;
        private MockedRequest _orderMockedRequest;
        private MockedRequest _inquiryMockedRequest;
        private BeatInfo _orderBeatInfo;
        private BeatInfo _inquiryBeatInfo;

        public BeatReactorTest()
        {
            _config = new NamingConfig
            {
                EndPoint = "http://localhost:8848",
                ServerAddr = new List<string>() { "http://localhost:8848" },
                Namespace = "vip56"
            };

            _mockHttp = new MockHttpMessageHandler();

            _orderBeatInfo = new BeatInfo();
            _orderBeatInfo.Port = 5000;
            _orderBeatInfo.Ip = "192.168.1.50";
            _orderBeatInfo.Weight = 1;
            _orderBeatInfo.ServiceName = "tms_order_v1";
            _orderBeatInfo.Cluster = "tms";
            _orderBeatInfo.MetaData.Add("k1", "v1");
            _orderBeatInfo.Scheduled = true;
            _orderBeatInfo.PerId = 500;
            _orderBeatInfo.Stopped = false;

            _orderMockedRequest = _mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/beat")
                .WithQueryString(NamingProxy.BEAT_KEY, _orderBeatInfo.ToString())
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _orderBeatInfo.ServiceName)
                .Respond("application/json", "ok");

            _inquiryBeatInfo = new BeatInfo();
            _inquiryBeatInfo.Port = 5000;
            _inquiryBeatInfo.Ip = "192.168.1.51";
            _inquiryBeatInfo.Weight = 1;
            _inquiryBeatInfo.ServiceName = "tms_inquiry_v1";
            _inquiryBeatInfo.Cluster = "tms";
            _inquiryBeatInfo.MetaData.Add("k2", "v2");
            _inquiryBeatInfo.Scheduled = true;
            _inquiryBeatInfo.PerId = 500;
            _inquiryBeatInfo.Stopped = false;

            _inquiryMockedRequest = _mockHttp.When(HttpMethod.Put, _config.ServerAddr[0] + UtilAndComs.NACOS_URL_BASE + "/instance/beat")
                .WithQueryString(NamingProxy.BEAT_KEY, _inquiryBeatInfo.ToString())
                .WithQueryString(NamingProxy.NAMESPACE_ID_KEY, _config.Namespace)
                .WithQueryString(NamingProxy.SERVICE_NAME_KEY, _inquiryBeatInfo.ServiceName)
                .Respond("application/json", "ok");
        }

        private BeatReactor CreateBeat()
        {
            var proxy = new NamingProxy(_config, new FastHttp(FakeHttpClientFactory.Create(_mockHttp.ToHttpClient()), _config));
            return new BeatReactor(proxy);
        }

        [Fact]
        public void BeatOneFireOneTest()
        {
            var beat = CreateBeat();

            beat.AddBeatInfo(_orderBeatInfo.ServiceName, _orderBeatInfo);

            Thread.Sleep(50);

            Assert.Equal(1, _mockHttp.GetMatchCount(_orderMockedRequest));
        }

        [Fact]
        public void BeatOneFireMoreTest()
        {
            var beat = CreateBeat();

            beat.AddBeatInfo(_orderBeatInfo.ServiceName, _orderBeatInfo);

            Thread.Sleep(550);

            Assert.Equal(2, _mockHttp.GetMatchCount(_orderMockedRequest));
        }

        [Fact]
        public void BeatTwoFireOneTest()
        {
            var beat = CreateBeat();

            beat.AddBeatInfo(_orderBeatInfo.ServiceName, _orderBeatInfo);
            beat.AddBeatInfo(_inquiryBeatInfo.ServiceName, _inquiryBeatInfo);

            Thread.Sleep(50);

            Assert.Equal(1, _mockHttp.GetMatchCount(_inquiryMockedRequest));
            Assert.Equal(1, _mockHttp.GetMatchCount(_orderMockedRequest));
        }

        [Fact]
        public void BeatTwoFireMoreTest()
        {
            var beat = CreateBeat();

            beat.AddBeatInfo(_orderBeatInfo.ServiceName, _orderBeatInfo);
            beat.AddBeatInfo(_inquiryBeatInfo.ServiceName, _inquiryBeatInfo);

            Thread.Sleep(550);

            Assert.Equal(2, _mockHttp.GetMatchCount(_inquiryMockedRequest));
            Assert.Equal(2, _mockHttp.GetMatchCount(_orderMockedRequest));
        }

        [Fact]
        public void BeatTwoAndRemoveOneTest()
        {
            var beat = CreateBeat();

            beat.AddBeatInfo(_orderBeatInfo.ServiceName, _orderBeatInfo);
            beat.AddBeatInfo(_inquiryBeatInfo.ServiceName, _inquiryBeatInfo);

            Thread.Sleep(50);

            beat.RemoveBeatInfo(_inquiryBeatInfo.ServiceName, _inquiryBeatInfo.Ip, _inquiryBeatInfo.Port);

            Thread.Sleep(550);

            Assert.Equal(2, _mockHttp.GetMatchCount(_orderMockedRequest));
            Assert.Equal(1, _mockHttp.GetMatchCount(_inquiryMockedRequest));
        }
    }
}
