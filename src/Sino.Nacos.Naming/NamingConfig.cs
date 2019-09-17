using Sino.Nacos.Naming.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class NamingConfig
    {
        public HttpConfig HttpConfig { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 服务地址列表
        /// </summary>
        public IList<string> ServerAddr { get; set; }
        
        /// <summary>
        /// 提供Nacos服务地址的地址（暂不支持规则形式）
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// 缓存路径
        /// </summary>
        public string CacheDir { get; set; } = "/nacos/naming/";

        /// <summary>
        /// 重启后从缓存恢复，默认关闭
        /// </summary>
        public bool LoadCacheAtStart { get; set; } = false;
    }
}
