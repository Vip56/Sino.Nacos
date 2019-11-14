using Microsoft.Extensions.Configuration;
using Sino.Nacos.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NacosConfigServiceCollectionExtensions
    {
        /// <summary>
        /// 启动配置服务
        /// </summary>
        public static IServiceCollection AddNacosConfig(this IServiceCollection services, IConfigurationSection section)
        {
            var mainCfg = new ConfigParam();
            section.Bind(mainCfg);
            return AddNacosConfig(services, mainCfg);
        }

        /// <summary>
        /// 启动配置服务
        /// </summary>
        public static IServiceCollection AddNacosConfig(this IServiceCollection services, ConfigParam config)
        {
            services.AddSingleton(config);
            services.AddSingleton<IConfigService, NacosConfigService>();

            return services;
        }
    }
}
