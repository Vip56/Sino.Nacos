using System;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置上下文接口
    /// </summary>
    public interface IConfigContext
    {
        /// <summary>
        /// 通过Key获取参数
        /// </summary>
        Object GetParameter(string key);

        /// <summary>
        /// 设置参数
        /// </summary>
        void SetParameter(string key, object value);
    }
}
