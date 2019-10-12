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

        public bool Initializing { get; set; }

        public string MD5 { get; private set; }

        public string Tenant { get; private set; }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                MD5 = CryptoUtils.GetMD5String(value);
            }
        }

        public void AddListener(IListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));


        }
    }
}
