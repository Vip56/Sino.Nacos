using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置响应
    /// </summary>
    public class ConfigResponse : IConfigResponse
    {
        private Dictionary<string, object> param = new Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Tenant
        {
            get
            {
                return param["tenant"] as string;
            }
            set
            {
                param.Add("tenant", value);
            }
        }

        /// <summary>
        /// 数据编号
        /// </summary>
        public string DataId
        {
            get
            {
                return param["dataId"] as string;
            }
            set
            {
                param.Add("dataId", value);
            }
        }

        /// <summary>
        /// 分组
        /// </summary>
        public string Group
        {
            get
            {
                return param["group"] as string;
            }
            set
            {
                param.Add("group", value);
            }
        }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get
            {
                return param["content"] as string;
            }
            set
            {
                param.Add("content", value);
            }
        }

        public IConfigContext getConfigContext()
        {
            return configContext;
        }

        public object getParameter(string key)
        {
            return param[key];
        }
    }
}
