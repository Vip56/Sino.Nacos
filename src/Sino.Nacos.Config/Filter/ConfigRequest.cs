using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Filter
{
    public class ConfigRequest : IConfigRequest
    {
        private Dictionary<string, object> _param = new Dictionary<string, object>();

        private IConfigContext _configContext = new ConfigContext();

        public string Tenant
        {
            get
            {
                if (_param.ContainsKey("tenant"))
                    return _param["tenant"] as string;
                return null;
            }
            set
            {
                _param.Add("tenant", value);
            }
        }

        public string DataId
        {
            get
            {
                if (_param.ContainsKey("dataId"))
                    return _param["dataId"] as string;
                return null;
            }
            set
            {
                _param.Add("dataId", value);
            }
        }

        public string Group
        {
            get
            {
                if (_param.ContainsKey("group"))
                    return _param["group"] as string;
                return null;
            }
            set
            {
                _param.Add("group", value);
            }
        }

        public string Content
        {
            get
            {
                if (_param.ContainsKey("content"))
                    return _param["content"] as string;
                return null;
            }
            set
            {
                _param.Add("content", value);
            }
        }

        public object GetParamter(string key)
        {
            if (_param.ContainsKey(key))
                return _param[key];
            return null;
        }

        public IConfigContext GetConfigContext()
        {
            return _configContext;
        }
    }
}
