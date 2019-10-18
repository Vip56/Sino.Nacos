using NLog;
using Sino.Nacos.Config.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Core
{
    public class CacheData
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _content;
        private ConcurrentList<ManagerListenerWrap> _listeners;
        private string _name;
        
        public string DataId { get; set; }
        public string Group { get; set; }
        public string Tenant { get; set; }

        public bool Initializing { get; set; }

        public string MD5 { get; private set; }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                MD5 = GetMD5String(value);
            }
        }

        public void AddListener(Action<string> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var wrap = new ManagerListenerWrap(listener, MD5);
            if (_listeners.AddIfNotExist(wrap))
            {
                _logger.Info($"[{_name}] [add-listener] ok, tenant={Tenant}, dataId={DataId}, group={Group}, cnt={_listeners.Count}");
            }
        }

        public void RemoveListener(Action<string> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            var wrap = new ManagerListenerWrap(listener, MD5);
            if (_listeners.Remove(wrap))
            {
                _logger.Info($"[{_name}] [remove-listener] ok, dataId={DataId}, group={Group}, cnt={_listeners.Count}");
            }
        }

        public static string GetMD5String(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            return CryptoUtils.GetMD5String(content);
        }
    }
}
