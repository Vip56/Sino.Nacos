using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    public class VirtualFilterChain : IConfigFilterChain
    {
        private IList<IConfigFilter> _additionalFilters;

        private int _currentPosition = 0;

        public VirtualFilterChain(IList<IConfigFilter> additionalFilters)
        {
            _additionalFilters = additionalFilters;
        }

        public void DoFilter(IConfigRequest request, IConfigResponse response)
        {
            if (_currentPosition != _additionalFilters.Count)
            {
                _currentPosition++;
                var nextFilter = _additionalFilters[_currentPosition - 1];
                nextFilter.DoFilter(request, response, this);
            }
        }
    }
}
