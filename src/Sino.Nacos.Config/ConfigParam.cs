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
        /// 内容路径
        /// </summary>
        public string ContentPath { get; set; }

        /// <summary>
        /// 集群名称
        /// </summary>
        public string ClusterName { get; set; }

        /// <summary>
        /// 提供Nacos服务地址的地址
        /// </summary>
        public string EndPoint { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public long ConnectionTimeout { get; set; }

        /// <summary>
        /// 配置缓存文件根路径
        /// </summary>
        public string LocalFileRoot { get; set; }
    }
}
