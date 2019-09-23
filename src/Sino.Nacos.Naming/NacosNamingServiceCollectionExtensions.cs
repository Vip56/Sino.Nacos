using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Sino.Nacos.Naming;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NacosNamingServiceCollectionExtensions
    {
        /// <summary>
        /// 启动服务注册与发现
        /// </summary>
        public static IServiceCollection AddNacosNaming(this IServiceCollection services, IConfigurationSection section)
        {
            var mainCfg = new NamingConfig();
            section.Bind(mainCfg);
            return AddNacosNaming(services, mainCfg);
        }

        /// <summary>
        /// 启动服务注册与发现
        /// </summary>
        public static IServiceCollection AddNacosNaming(this IServiceCollection services, NamingConfig config)
        {
            services.AddSingleton(config);
            services.AddSingleton<INamingService, NacosNamingService>();

            return services;
        }

        /// <summary>
        /// 自动注册服务
        /// </summary>
        public static IApplicationBuilder AutoRegisterNacosNaming(this IApplicationBuilder app)
        {
            var namingService = app.ApplicationServices.GetService<INamingService>();

            namingService.AutoRegister().Wait();

            return app;
        }
    }
}
