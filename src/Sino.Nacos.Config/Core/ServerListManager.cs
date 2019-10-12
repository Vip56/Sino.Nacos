using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sino.Nacos.Config.Core
{
    public class ServerListManager
    {
        private static string HTTPS = "https://";
        private static string HTTP = "http://";

        public const string DEFAULT_NAME = "default";
        public const string CUSTOM_NAME = "custom";
        public const string FIXED_NAME = "fixed";

        public static int TIMEOUT = 5000;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _isFixed;
        private bool _isStarted;
        private string _name;
        private string _namespaceName;
        private volatile List<string> _serverUrls = new List<string>();

        public ServerListManager()
        {
            _isFixed = false;
            _isStarted = false;
            _name = DEFAULT_NAME;
        }

        public ServerListManager(IList<string> fixedList)
            : this(fixedList, null) { }

        public ServerListManager(IList<string> fixedList, string namespaceName)
        {
            _isFixed = true;
            _isStarted = true;
            var serverAddrs = new List<string>();
            foreach(var serverAddr in fixedList)
            {
                string[] serverAddrArr = serverAddr.Split(':');
                if (serverAddrArr.Length == 1)
                {
                    serverAddrs.Add(serverAddrArr[0] + ':' + UtilAndComs.DEFAULT_SERVER_PORT);
                }
                else
                {
                    serverAddrs.Add(serverAddr);
                }
            }
            _serverUrls = serverAddrs;

            if (string.IsNullOrEmpty(namespaceName))
            {
                _name = FIXED_NAME + "-" + GetFixedNameSuffix(serverAddrs.ToArray());
            }
            else
            {
                _namespaceName = namespaceName;
                _name = FIXED_NAME + "-" + GetFixedNameSuffix(serverAddrs.ToArray()) + "-" + _namespaceName;
            }
        }

        public ServerListManager(string host, int port)
        {

        }

        private string GetFixedNameSuffix(params string[] serverIps)
        {
            StringBuilder sb = new StringBuilder();
            string split = "";
            foreach(string serverIp in serverIps)
            {
                sb.Append(split);
                var ip = Regex.Replace(serverIp, "http(s)?://", "");
                sb.Append(ip.Replace(':', '_'));
                split = "-";
            }
            return sb.ToString();
        }
    }
}
