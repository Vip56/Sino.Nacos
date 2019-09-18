using Sino.Nacos.Naming.Core;
using Sino.Nacos.Naming.Listener;
using Sino.Nacos.Naming.Model;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace NacosNamingUnitTest
{
    public class EventDispatcherTest
    {
        [Fact]
        public async Task AddAndFireOneListenerTest()
        {
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            ServiceInfo serviceInfo = new ServiceInfo("tms_order_v1", "tms");
            serviceInfo.GroupName = "test";
            serviceInfo.Hosts.Add(instance);

            TaskCompletionSource<NamingEvent> source = new TaskCompletionSource<NamingEvent>();

            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddListener(serviceInfo, serviceInfo.Clusters, x =>
            {
                var t = x as NamingEvent;
                source.SetResult(t);
            });

            eventDispatcher.ServiceChanged(serviceInfo);

            NamingEvent result = null;
            if (source.Task.Wait(1000))
            {
                result = await source.Task;
            }
            else
            {
                Assert.True(false);
            }

            Assert.Equal(result.Clusters, serviceInfo.Clusters);
            Assert.Equal(result.GroupName, serviceInfo.GroupName);
            Assert.Equal(result.ServiceName, serviceInfo.Name);
            Assert.Equal(result.Instances.Count, serviceInfo.Hosts.Count);
            Assert.Equal(result.Instances.First().ClusterName, serviceInfo.Hosts.First().ClusterName);
            Assert.Equal(result.Instances.First().Ip, serviceInfo.Hosts.First().Ip);
            Assert.Equal(result.Instances.First().Port, serviceInfo.Hosts.First().Port);
            Assert.Equal(result.Instances.First().Weight, serviceInfo.Hosts.First().Weight);
            Assert.Equal(result.Instances.First().Enable, serviceInfo.Hosts.First().Enable);
            Assert.Equal(result.Instances.First().Healthy, serviceInfo.Hosts.First().Healthy);
            Assert.Equal(result.Instances.First().Ephemeral, serviceInfo.Hosts.First().Ephemeral);
            Assert.Equal(result.Instances.First().Metadata.Count, serviceInfo.Hosts.First().Metadata.Count);
        }

        [Fact]
        public void AddAndRemoveListenerTest()
        {
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            ServiceInfo serviceInfo = new ServiceInfo("tms_order_v1", "tms");
            serviceInfo.GroupName = "test";
            serviceInfo.Hosts.Add(instance);

            TaskCompletionSource<NamingEvent> source = new TaskCompletionSource<NamingEvent>();

            var eventDispatcher = new EventDispatcher();

            Action<IEvent> listener = x =>
            {
                var t = x as NamingEvent;
                source.SetResult(t);
            };

            eventDispatcher.AddListener(serviceInfo, serviceInfo.Clusters, listener);

            eventDispatcher.RemoveListener(serviceInfo.Name, serviceInfo.Clusters, listener);

            eventDispatcher.ServiceChanged(serviceInfo);

            if (source.Task.Wait(1000))
            {
                Assert.True(false);
            }
            else
            {
                Assert.True(true);
            }
        }

        [Fact]
        public async Task AddAndFireMoreListenerTest()
        {
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            ServiceInfo serviceInfo = new ServiceInfo("tms_order_v1", "tms");
            serviceInfo.GroupName = "test";
            serviceInfo.Hosts.Add(instance);

            TaskCompletionSource<NamingEvent> source = new TaskCompletionSource<NamingEvent>();

            Action<IEvent> listener = x =>
            {
                var t = x as NamingEvent;
                source.SetResult(t);
            };

            Instance instance2 = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.102";
            instance.Port = 5000;
            instance.Weight = 6.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("k1", "v1");

            ServiceInfo serviceInfo2 = new ServiceInfo("tms_inquiry_v1", "tms");
            serviceInfo2.GroupName = "test";
            serviceInfo2.Hosts.Add(instance2);

            TaskCompletionSource<NamingEvent> source2 = new TaskCompletionSource<NamingEvent>();

            Action<IEvent> listener2 = x =>
            {
                var t = x as NamingEvent;
                source2.SetResult(t);
            };

            var eventDispatcher = new EventDispatcher();

            eventDispatcher.AddListener(serviceInfo, serviceInfo.Clusters, listener);
            eventDispatcher.AddListener(serviceInfo2, serviceInfo2.Clusters, listener2);

            eventDispatcher.ServiceChanged(serviceInfo);

            if (source2.Task.Wait(500))
            {
                Assert.True(false);
            }
            else
            {
                if(!source.Task.Wait(500))
                {
                    Assert.True(false);
                }
            }

            var result = await source.Task;

            Assert.Equal(result.Clusters, serviceInfo.Clusters);
            Assert.Equal(result.GroupName, serviceInfo.GroupName);
            Assert.Equal(result.ServiceName, serviceInfo.Name);
            Assert.Equal(result.Instances.Count, serviceInfo.Hosts.Count);
            Assert.Equal(result.Instances.First().ClusterName, serviceInfo.Hosts.First().ClusterName);
            Assert.Equal(result.Instances.First().Ip, serviceInfo.Hosts.First().Ip);
            Assert.Equal(result.Instances.First().Port, serviceInfo.Hosts.First().Port);
            Assert.Equal(result.Instances.First().Weight, serviceInfo.Hosts.First().Weight);
            Assert.Equal(result.Instances.First().Enable, serviceInfo.Hosts.First().Enable);
            Assert.Equal(result.Instances.First().Healthy, serviceInfo.Hosts.First().Healthy);
            Assert.Equal(result.Instances.First().Ephemeral, serviceInfo.Hosts.First().Ephemeral);
            Assert.Equal(result.Instances.First().Metadata.Count, serviceInfo.Hosts.First().Metadata.Count);

            TaskCompletionSource<NamingEvent> source3 = new TaskCompletionSource<NamingEvent>();

            eventDispatcher.AddListener(serviceInfo2, serviceInfo2.Clusters, x =>
            {
                var t = x as NamingEvent;
                source3.SetResult(t);
            });
            eventDispatcher.ServiceChanged(serviceInfo2);

            if (!source3.Task.Wait(500) || !source2.Task.Wait(500))
            {
                Assert.True(false);
            }

            var result2 = await source3.Task;

            Assert.Equal(result2.Clusters, serviceInfo2.Clusters);
            Assert.Equal(result2.GroupName, serviceInfo2.GroupName);
            Assert.Equal(result2.ServiceName, serviceInfo2.Name);
            Assert.Equal(result2.Instances.Count, serviceInfo2.Hosts.Count);
            Assert.Equal(result2.Instances.First().ClusterName, serviceInfo2.Hosts.First().ClusterName);
            Assert.Equal(result2.Instances.First().Ip, serviceInfo2.Hosts.First().Ip);
            Assert.Equal(result2.Instances.First().Port, serviceInfo2.Hosts.First().Port);
            Assert.Equal(result2.Instances.First().Weight, serviceInfo2.Hosts.First().Weight);
            Assert.Equal(result2.Instances.First().Enable, serviceInfo2.Hosts.First().Enable);
            Assert.Equal(result2.Instances.First().Healthy, serviceInfo2.Hosts.First().Healthy);
            Assert.Equal(result2.Instances.First().Ephemeral, serviceInfo2.Hosts.First().Ephemeral);
            Assert.Equal(result2.Instances.First().Metadata.Count, serviceInfo2.Hosts.First().Metadata.Count);
        }

        [Fact]
        public void AddAndFireWithOneEventMoreListenerTest()
        {
            Instance instance = new Instance();
            instance.ClusterName = "tms";
            instance.Ip = "192.168.1.101";
            instance.Port = 5000;
            instance.Weight = 5.0;
            instance.Enable = true;
            instance.Healthy = true;
            instance.Ephemeral = true;
            instance.Metadata.Add("t1", "v1");

            ServiceInfo serviceInfo = new ServiceInfo("tms_order_v1", "tms");
            serviceInfo.GroupName = "test";
            serviceInfo.Hosts.Add(instance);

            TaskCompletionSource<NamingEvent> source = new TaskCompletionSource<NamingEvent>();
            TaskCompletionSource<NamingEvent> source2 = new TaskCompletionSource<NamingEvent>();

            var eventDispatcher = new EventDispatcher();

            eventDispatcher.AddListener(serviceInfo, serviceInfo.Clusters, x =>
            {
                var t = x as NamingEvent;
                source.SetResult(t);
            });

            Action<IEvent> listener = x =>
            {
                var t = x as NamingEvent;
                source2.SetResult(t);
            };
            eventDispatcher.AddListener(serviceInfo, serviceInfo.Clusters, listener);

            eventDispatcher.RemoveListener(serviceInfo.Name, serviceInfo.Clusters, listener);

            eventDispatcher.ServiceChanged(serviceInfo);

            if (source2.Task.Wait(500))
            {
                Assert.True(false);
            }

            if (!source.Task.Wait(500))
            {
                Assert.True(false);
            }
        }
    }
}
