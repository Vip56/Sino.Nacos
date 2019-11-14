using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 过滤器配置参数接口
    /// </summary>
    public interface IFilterConfig
    {
        /// <summary>
        /// 获取过滤器名称
        /// </summary>
        string GetFilterName();

        /// <summary>
        /// 获取初始化参数
        /// </summary>
        object GetInitParameter(string name);
    }
}
