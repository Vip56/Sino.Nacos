using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    public interface IConfigFilterChain
    {
        /// <summary>
        /// 过滤
        /// </summary>
        void DoFilter(IConfigRequest request, IConfigResponse response);
    }
}
