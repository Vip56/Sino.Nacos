using Moq;
using Sino.Nacos.Naming.Core;
using Sino.Nacos.Naming.Model;
using System;
using System.Collections.Generic;
using Sino.Nacos.Naming.Utils;
using Xunit;
using Sino.Nacos.Naming.Backups;
using System.IO;
using Sino.Nacos.Naming.Cache;
using System.Threading;
using System.Linq;
using Sino.Nacos.Naming;
using System.Collections.Concurrent;

namespace NacosNamingUnitTest
{
    public class FailoverReactorTest
    {
        private IHostReactor _hostReactor;
        private ConcurrentDictionary<string, ServiceInfo> _serviceMap;

        public FailoverReactorTest()
        {
            _serviceMap = new ConcurrentDictionary<string, ServiceInfo>();
            var orderServiceInfo = new ServiceInfo();
            orderServiceInfo.Name = "tms_order_v1";
            orderServiceInfo.GroupName = "tms";
            orderServiceInfo.Clusters = "test";
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
            orderServiceInfo.Hosts.Add(orderInstance);
            orderServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            var inquiryServiceInfo = new ServiceInfo();
            inquiryServiceInfo.Name = "tms_inquiry_v1";
            inquiryServiceInfo.GroupName = "tms";
            inquiryServiceInfo.Clusters = "test";
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
            inquiryServiceInfo.Hosts.Add(inquiryInstance);
            inquiryServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            _serviceMap.TryAdd(orderServiceInfo.GetKey(), orderServiceInfo);
            _serviceMap.TryAdd(inquiryServiceInfo.GetKey(), inquiryServiceInfo);

            var mockHostReactor = new Mock<IHostReactor>();
            mockHostReactor.Setup(x => x.GetServiceInfoMap()).Returns(_serviceMap);

            _hostReactor = mockHostReactor.Object;
        }

        [Fact]
        public void FailoverFileSaveTest()
        {
            FailoverReactor.DISK_FILE_WRITER_DUETIME = 0;
            FailoverReactor.DIR_NOT_FOUND_DUETIME = 0;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var failover = new FailoverReactor(_hostReactor, path);

            Thread.Sleep(100);

            var infos = DiskCache.GetServiceInfos(Path.Combine(path, FailoverReactor.FAILOVER_PATH));

            Assert.False(failover.IsFailoverSwitch());
            Assert.NotNull(infos);
            Assert.Equal(2, infos.Count);

            ServiceInfo infoOne = infos.First().Value;
            ServiceInfo infoTwo;
            if (infos.First().Key == _serviceMap.First().Key)
            {
                infoTwo = _serviceMap.First().Value;
            }
            else
            {
                infoTwo = _serviceMap.ElementAt(1).Value;
            }

            Assert.Equal(infoOne.AllIPs, infoTwo.AllIPs);
            Assert.Equal(infoOne.CacheMillis, infoTwo.CacheMillis);
            Assert.Equal(infoOne.CheckSum, infoTwo.CheckSum);
            Assert.Equal(infoOne.Clusters, infoTwo.Clusters);
            Assert.Equal(infoOne.GroupName, infoTwo.GroupName);
            Assert.Equal(infoOne.Hosts.Count, infoTwo.Hosts.Count);
            Assert.Equal(infoOne.Hosts[0].ClusterName, infoTwo.Hosts[0].ClusterName);
            Assert.Equal(infoOne.Hosts[0].Enable, infoTwo.Hosts[0].Enable);
            Assert.Equal(infoOne.Hosts[0].Ephemeral, infoTwo.Hosts[0].Ephemeral);
            Assert.Equal(infoOne.Hosts[0].Healthy, infoTwo.Hosts[0].Healthy);
            Assert.Equal(infoOne.Hosts[0].InstanceId, infoTwo.Hosts[0].InstanceId);
            Assert.Equal(infoOne.Hosts[0].Ip, infoTwo.Hosts[0].Ip);
            Assert.Equal(infoOne.Hosts[0].Metadata.Count, infoTwo.Hosts[0].Metadata.Count);
            Assert.Equal(infoOne.Hosts[0].Port, infoTwo.Hosts[0].Port);
            Assert.Equal(infoOne.Hosts[0].ServiceName, infoTwo.Hosts[0].ServiceName);
            Assert.Equal(infoOne.Hosts[0].Weight, infoTwo.Hosts[0].Weight);
            Assert.Equal(infoOne.LastRefTime, infoTwo.LastRefTime);
            Assert.Equal(infoOne.Name, infoTwo.Name);

            Directory.Delete(Path.Combine(path, FailoverReactor.FAILOVER_PATH), true);
        }

        [Fact]
        public void FailoverServiceInfoUpdateTest()
        {
            FailoverReactor.DISK_FILE_WRITER_DUETIME = 0;
            FailoverReactor.DISK_FILE_WRITER_PERIOD = 500;
            FailoverReactor.DIR_NOT_FOUND_DUETIME = 0;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var failover = new FailoverReactor(_hostReactor, path);

            var feeServiceInfo = new ServiceInfo();
            feeServiceInfo.Name = "tms_fee_v1";
            feeServiceInfo.GroupName = "tms";
            feeServiceInfo.Clusters = "test";
            var feeInstance = new Instance()
            {
                InstanceId = "1",
                Ip = "192.168.1.55",
                Port = 5000,
                Weight = 1,
                ClusterName = "test",
                ServiceName = "tms_fee_v1"
            };
            feeInstance.Metadata.Add("k1", "v1");
            feeServiceInfo.Hosts.Add(feeInstance);
            feeServiceInfo.LastRefTime = DateTime.Now.GetTimeStamp();

            _serviceMap.TryAdd(feeServiceInfo.GetKey(), feeServiceInfo);

            Thread.Sleep(500);

            var infos = DiskCache.GetServiceInfos(Path.Combine(path, FailoverReactor.FAILOVER_PATH));

            Assert.NotNull(infos);
            Assert.False(failover.IsFailoverSwitch());
            Assert.Equal(3, infos.Count);

            Directory.Delete(Path.Combine(path, FailoverReactor.FAILOVER_PATH), true);
        }

        [Fact]
        public void FailoverWithSwithcOnTest()
        {
            FailoverReactor.DISK_FILE_WRITER_DUETIME = 0;
            FailoverReactor.DISK_FILE_WRITER_PERIOD = 500;
            FailoverReactor.DIR_NOT_FOUND_DUETIME = 0;
            FailoverReactor.SWITCH_REFRESHER_PERIOD = 500;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string failoverPath = Path.Combine(path, FailoverReactor.FAILOVER_PATH);
            string failoverSwitchPath = Path.Combine(failoverPath, UtilAndComs.FAILOVER_SWITCH);

            DiskCache.WriteFile(failoverSwitchPath, "1" + DiskCache.GetLineSeparator());

            var failover = new FailoverReactor(_hostReactor, path);

            Thread.Sleep(100);

            var info = failover.GetService(_serviceMap.First().Value.GetKey());

            Assert.True(failover.IsFailoverSwitch());
            Assert.NotNull(info);

            DiskCache.WriteFile(failoverSwitchPath, "0" + DiskCache.GetLineSeparator());

            Thread.Sleep(550);

            Assert.False(failover.IsFailoverSwitch());

            Directory.Delete(Path.Combine(path, FailoverReactor.FAILOVER_PATH), true);
        }
    }
}
