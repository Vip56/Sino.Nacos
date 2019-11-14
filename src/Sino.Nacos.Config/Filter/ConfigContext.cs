using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    public class ConfigContext : IConfigContext
    {
        private Dictionary<string, object> _param = new Dictionary<string, object>();

        public object GetParameter(string key)
        {
            if (_param.ContainsKey(key))
                return _param[key];

            return null;
        }

        public void SetParameter(string key, object value)
        {
            _param.Add(key, value);
        }
    }
}
