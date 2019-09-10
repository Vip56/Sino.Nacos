using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    /// <summary>
    /// 配置上下文
    /// </summary>
    public class ConfigContext : IConfigContext
    {
        private Dictionary<string, object> param = new Dictionary<string, object>();

        public object GetParameter(string key)
        {
            return param[key];
        }

        public void SetParameter(string key, object value)
        {
            param.Add(key, value);
        }
    }
}
