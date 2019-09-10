using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
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
        /// <param name="filterChan">调用链</param>
        void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChan);

        void Deploy();

        /// <summary>
        /// 优先级
        /// </summary>
        int GetOrder();

        /// <summary>
        /// 过滤器名称
        /// </summary>
        /// <returns></returns>
        string GetFilterName();
    }
}
