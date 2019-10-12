using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置过滤接口
    /// </summary>
    public interface IConfigFilter
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init(IFilterConfig filterConfig);

        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="response">回应</param>
        /// <param name="filterChain">过滤链</param>
        void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain);

        void Deploy();

        int GetOrder();

        string GetFilterName();
    }
}
