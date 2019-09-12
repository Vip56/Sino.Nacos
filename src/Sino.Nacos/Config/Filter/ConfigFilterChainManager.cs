using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;

namespace Sino.Nacos.Config.Filter
{
    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private List<IConfigFilter> filters = new List<IConfigFilter>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ConfigFilterChainManager AddFilter(IConfigFilter filter)
        {
            int i = 0;
            while (i < this.filters.Count)
            {
                IConfigFilter currentValue = filters[i];
                if (currentValue.GetFilterName().Equals(filter.GetFilterName()))
                {
                    break;
                }
                if (filter.GetOrder() >= currentValue.GetOrder() && i < filters.Count)
                {
                    i++;
                }
                else
                {
                    filters.Insert(i, filter);
                    break;
                }

                if (i == filters.Count)
                {
                    filters.Add(filter);
                }
            }
            return this;
        }

        public void DoFilter(IConfigRequest request, IConfigResponse response)
        {
            new VirtualFilterChain(filters).DoFilter(request, response);
        }

        private class VirtualFilterChain : IConfigFilterChain
        {
            private List<IConfigFilter> additionalFilters;
            private int currentPosition = 0;

            public VirtualFilterChain(List<IConfigFilter> additionalFilters)
            {
                this.additionalFilters = additionalFilters;
            }

            public void DoFilter(IConfigRequest request, IConfigResponse response)
            {
                if (currentPosition != additionalFilters.Count)
                {
                    currentPosition++;
                    IConfigFilter nextFilter = additionalFilters[currentPosition - 1];
                    nextFilter.DoFilter(request, response, this);
                }
            }
        }
    }
}
