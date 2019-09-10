using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 过滤器链接口
    /// </summary>
    public interface IConfigFilterChain
    {
        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="response">回应</param>
        void DoFilter(IConfigRequest request, IConfigResponse response);
    }
}
