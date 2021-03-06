﻿using Sino.Nacos.Naming.Net;
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

        /// <summary>
        /// 连接超时时间，默认3000毫秒
        /// </summary>
        public int ConnectionTimeout { get; set; } = 3000;

        /// <summary>
        /// 公钥
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 是否自动注册
        /// </summary>
        public bool AutoRegister { get; set; }

        /// <summary>
        /// 注册的服务名
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 注册的组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 自动注册IP前缀
        /// </summary>
        public string IpPrefix { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }
    }
}
