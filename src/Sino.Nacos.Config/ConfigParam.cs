using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config
{
    public class ConfigParam
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
        /// 提供Nacos服务地址的地址
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// 配置缓存文件根路径
        /// </summary>
        public string LocalFileRoot { get; set; }

        /// <summary>
        /// 拉取等待时长
        /// </summary>
        public long ConfigLongPollTimeout { get; set; } = 30000;

        /// <summary>
        /// 间隔时间
        /// </summary>
        public int ConfigRetryTime { get; set; } = 2000;

        /// <summary>
        /// 启动远程同步配置
        /// </summary>
        public bool EnableRemoteSyncConfig { get; set; } = false;
    }
}
