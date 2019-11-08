using Sino.Nacos.Config.Core;
using Sino.Nacos.Config.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace NacosConfigUnitTest
{
    public class CacheDataTest
    {
        private LocalConfigInfoProcessor _localConfigInfoProcessor;
        private ConfigFilterChainManager _configFilterChainManager;

        public CacheDataTest()
        {
            _localConfigInfoProcessor = new LocalConfigInfoProcessor(AppDomain.CurrentDomain.BaseDirectory);
            _configFilterChainManager = new ConfigFilterChainManager();
        }

        ~CacheDataTest()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalConfigInfoProcessor.SnapshotPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        [Fact]
        public void AddListenerAndRemoveTest()
        {
            int fire = 0;
            string content = string.Empty;

            var data = new CacheData(_configFilterChainManager,
                _localConfigInfoProcessor,
                "tms", "rongyun", "core");

            Assert.Equal("rongyun", data.DataId);
            Assert.Equal("core", data.Group);
            Assert.Equal(string.Empty, data.Content);

            Action<string> listener = x =>
            {
                content = x;
                fire++;
            };

            data.AddListener(listener);

            Assert.Equal(string.Empty, content);
            Assert.Equal(0, fire);

            var listeners = data.GetListeners();

            Assert.Equal(1, listeners.Count);

            data.CheckListenerMD5();

            Assert.Equal(string.Empty, content);
            Assert.Equal(0, fire);

            data.RemoveListener(listener);

            listeners = data.GetListeners();

            Assert.Equal(0, listeners.Count);
        }

        [Fact]
        public void AddListenerAndFireTest()
        {
            int fire1 = 0;
            int fire2 = 0;
            string content1 = string.Empty;
            string content2 = string.Empty;

            var data = new CacheData(_configFilterChainManager,
                _localConfigInfoProcessor,
                "tms", "rongyun", "core");

            Action<string> listener1 = x =>
            {
                fire1++;
                content1 = x;
            };

            Action<string> listener2 = x =>
            {
                fire2++;
                content2 = x;
            };

            data.AddListener(listener1);

            data.CheckListenerMD5();
            Assert.Equal(0, fire1);
            Assert.Equal(0, fire2);
            Assert.Equal(string.Empty, content1);
            Assert.Equal(string.Empty, content2);

            data.Content = "test1";
            data.CheckListenerMD5();

            Assert.Equal(1, fire1);
            Assert.Equal("test1", content1);
            Assert.Equal(0, fire2);
            Assert.Equal(string.Empty, content2);

            data.AddListener(listener1);
            data.AddListener(listener2);

            data.Content = "test2";
            data.CheckListenerMD5();
            Assert.Equal(2, fire1);
            Assert.Equal("test2", content1);
            Assert.Equal(1, fire2);
            Assert.Equal("test2", content2);

            data.RemoveListener(listener1);

            data.Content = "test3";
            data.CheckListenerMD5();
            Assert.Equal(2, fire1);
            Assert.Equal("test2", content1);
            Assert.Equal(2, fire2);
            Assert.Equal("test3", content2);
        }
    }
}
