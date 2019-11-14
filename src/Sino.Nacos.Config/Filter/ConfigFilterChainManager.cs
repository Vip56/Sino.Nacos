using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置过滤链管理
    /// </summary>
    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private IList<IConfigFilter> _filters = new List<IConfigFilter>();

        public ConfigFilterChainManager AddFilter(IConfigFilter filter)
        {
            lock(_filters)
            {
                int i = 0;
                while(i < _filters.Count)
                {
                    var currentValue = _filters[i];
                    if (currentValue.GetFilterName() == filter.GetFilterName())
                    {
                        break;
                    }
                    if (filter.GetOrder() >= currentValue.GetOrder() && i < _filters.Count)
                    {
                        i++;
                    }
                    else
                    {
                        _filters.Insert(i, filter);
                        break;
                    }
                }

                if (i == _filters.Count)
                {
                    _filters.Add(filter);
                }

                return this;
            }
        }

        public void DoFilter(IConfigRequest request, IConfigResponse response)
        {
            new VirtualFilterChain(_filters).DoFilter(request, response);
        }
    }
}