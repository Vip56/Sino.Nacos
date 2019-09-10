using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置回应接口
    /// </summary>
    public interface IConfigResponse
    {
        /// <summary>
        /// 获取参数
        /// </summary>
        object getParameter(string key);

        /// <summary>
        /// 获取配置上下文
        /// </summary>
        IConfigContext getConfigContext();
    }
}
