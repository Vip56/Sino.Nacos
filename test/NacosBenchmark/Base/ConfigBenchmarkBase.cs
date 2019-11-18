using Microsoft.Extensions.DependencyInjection;
using Sino.Nacos.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace NacosBenchmark
{
    public class ConfigBenchmarkBase
    {
        protected IServiceProvider ServiceProvider { get; set; }

        protected IConfigService ConfigService { get; set; }

        public ConfigBenchmarkBase()
        {
            var collection = new ServiceCollection();
            collection.AddHttpClient();
            collection.AddNacosConfig(new ConfigParam
            {
                ServerAddr = new List<string>
                {
                    "http://localhost:8848"
                },
                LocalFileRoot = AppDomain.CurrentDomain.BaseDirectory
            });

            ServiceProvider = collection.BuildServiceProvider();

            ConfigService = ServiceProvider.GetService<IConfigService>();
        }
    }
}
