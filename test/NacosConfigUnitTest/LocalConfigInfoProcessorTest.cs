using Sino.Nacos.Config.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace NacosConfigUnitTest
{
    public class LocalConfigInfoProcessorTest
    {
        public LocalConfigInfoProcessorTest()
        {

        }

        [Fact]
        public void SnapshotFileSaveTest()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var localConfig = new LocalConfigInfoProcessor(path);

            string content = localConfig.GetFailover("tms", "rongyun", "default", "core");

            Assert.Equal(string.Empty, content);

            localConfig.SaveSnapshot("tms", "rongyun", "default", "core", "rongyuntest");

            localConfig.SaveSnapshot("tms", "zgsign", "default", "core", "zgsigntest");

            content = localConfig.GetSnapshot("tms", "rongyun", "default", "core");

            Assert.Equal("rongyuntest", content);

            content = localConfig.GetSnapshot("tms", "zgsign", "default", "core");

            Assert.Equal("zgsigntest", content);

            localConfig.SaveSnapshot("tms", "zgsign", "default", "core", "");
            content = localConfig.GetSnapshot("tms", "zgsign", "default", "core");

            Assert.Equal(string.Empty, content);

            Directory.Delete(Path.Combine(path, LocalConfigInfoProcessor.SnapshotPath), true);
        }

        [Fact]
        public void SnapshotSwitchTest()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var localConfig = new LocalConfigInfoProcessor(path);

            localConfig.SaveSnapshot("tms", "rongyun", "default", "core", "rongyuntest");
            localConfig.SaveSnapshot("tms", "zgsign", "default", "core", "zgsigntest");

            localConfig.CleanAllSnapshot("tms");

            string content = localConfig.GetSnapshot("tms", "rongyun", "default", "core");
            Assert.Equal("rongyuntest", content);

            content = localConfig.GetSnapshot("ccp", "zgsign", "default", "core");
            Assert.Equal("zgsigntest", content);

            Directory.Delete(Path.Combine(path, LocalConfigInfoProcessor.SnapshotPath), true);
        }
    }
}
