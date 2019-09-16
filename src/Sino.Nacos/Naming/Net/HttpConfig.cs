using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Net
{
    public class HttpConfig
    {
        /// <summary>
        /// 连接超时时间，默认3000毫秒
        /// </summary>
        public int ConnectionTimeout { get; set; } = 3000;

        /// <summary>
        /// 服务器列表
        /// </summary>
        public IList<string> ServerList { get; set; }

        /// <summary>
        /// 目标节点
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; } = 8848;

        /// <summary>
        /// 名命空间（失效）
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 公钥
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// ip地址
        /// </summary>
        public string Ip { get; set; }
    }
}
