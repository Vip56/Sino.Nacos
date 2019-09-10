using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置上下文接口
    /// </summary>
    public interface IConfigContext
    {
        /// <summary>
        /// 获取上下文参数
        /// </summary>
        object GetParameter(string key);

        /// <summary>
        /// 设置上下文参数
        /// </summary>
        void SetParameter(string key, object value);
    }
}
