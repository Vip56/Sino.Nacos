using NLog;
using Sino.Nacos.Config.Exceptions;
using Sino.Nacos.Config.Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sino.Nacos.Config.Core
{
    public class ServerListManager
    {

        public const string DEFAULT_NAME = "default";
        public const string CUSTOM_NAME = "custom";
        public const string FIXED_NAME = "fixed";

        public static int TIMEOUT = 5000;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _isFixed;
        private bool _isStarted;
        private string _name;
        private string _namespaceName;
        private string _tenant;
        private volatile IList<string> _serverUrls = new List<string>();
        private string _serverListName;
        private string _endPoint;
        private FastHttp _http;

        public string AddressServerUrl { get; set; }

        public string ContentPath { get; private set; }

        public ServerListManager(ConfigParam config, IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _http = new FastHttp(httpClientFactory);
            _isStarted = false;
            _serverUrls = config.ServerAddr;
            _endPoint = config.EndPoint;
            ContentPath = config.ContentPath;
            _serverListName = config.ClusterName;

            if (_serverUrls != null && _serverUrls.Count > 0)
            {
                _isFixed = true;
                if (string.IsNullOrEmpty(config.Namespace))
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}";
                }
                else
                {
                    _namespaceName = config.Namespace;
                    _tenant = config.Namespace;
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}-{_namespaceName}";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_endPoint))
                {
                    throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "endpoint is blank");
                }
                _isFixed = false;
                if (string.IsNullOrEmpty(config.Namespace))
                {
                    _name = _endPoint;
                    AddressServerUrl = $"http://{_endPoint}/{ContentPath}/{_serverListName}";
                }
                else
                {
                    _namespaceName = config.Namespace;
                    _tenant = config.Namespace;
                    _name = $"{_endPoint}-{_namespaceName}";
                    AddressServerUrl = $"http://{_endPoint}/{ContentPath}/{_serverListName}?namespace={_namespaceName}";
                }
            }
        }

        private string GetFixedNameSuffix(IList<string> serverIps)
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

        public void Start()
        {
            lock(this)
            {
                if (_isStarted || _isFixed)
                {
                    return;
                }


            }
        }

        private async Task GetServerList()
        {

        }

        private async Task UpdateIfChanged(IList<string> newList)
        {

        }

        private async Task<IList<string>> GetApacheServerList(string url, string name)
        {
            string response = await _http.Request(url, null, null, Encoding.UTF8, HttpMethod.Get, 3000);

            if (DEFAULT_NAME == name)
            {

            }
            var lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
