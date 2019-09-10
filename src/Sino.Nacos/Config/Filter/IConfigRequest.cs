using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置请求接口
    /// </summary>
    public interface IConfigRequest
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        object getParameter(string key);

        /// <summary>
        /// 获取配置上下文
        /// </summary>
        IConfigContext getConfigContext();
    }
}
