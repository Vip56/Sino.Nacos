using NLog;
using Sino.Nacos.Config.Exceptions;
using Sino.Nacos.Config.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.RegularExpressions;

namespace Sino.Nacos.Config.Net
{
    public class ServerHttpAgent : IHttpAgent
    {
        public const string VERSION = "Nacos-CSharp-Client-v1.1.3";
        public const int REQUEST_DOMAIN_RETRY_COUNT = 3;
        public const string FIXED_NAME = "fixed";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IList<string> _serverList;
        private IList<string> _serversFromEndpoint;
        private string _endpoint;
        private string _nacosDomain;
        private ConfigParam _config;
        private FastHttp _http;

        private string _name;
        private string _tenant;

        /// <summary>
        /// 最后一次从EndPoint获取Nacos服务器的时间
        /// </summary>
        private long _lastSrvRefTime = 0;

        /// <summary>
        /// 从EndPoint获取最新Nacos服务器列表的响应间隔时间
        /// </summary>
        private int _vipSrvRefInterMillis = 30 * 60 * 1000;

        public ServerHttpAgent(ConfigParam config, FastHttp http)
        {
            _config = config;
            _http = http;
            Init(config);
        }

        private void Init(ConfigParam config)
        {
            _serverList = config.ServerAddr;
            _endpoint = config.EndPoint;
            if (_serverList != null && _serverList.Count == 1)
            {
                _nacosDomain = _serverList.First();
            }

            if (_serverList != null && _serverList.Count > 0)
            {
                if (string.IsNullOrEmpty(config.Namespace))
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverList)}";
                }
                else
                {
                    _tenant = config.Namespace;
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverList)}-{config.Namespace}";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_config.Namespace))
                {
                    _name = _endpoint;
                }
                else
                {
                    _tenant = config.Namespace;
                    _name = $"{_endpoint}-{config.Namespace}";
                }
            }

            InitRefreshSrvIfNeed();
        }

        private string GetFixedNameSuffix(IList<string> serverIps)
        {
            StringBuilder sb = new StringBuilder();
            string split = "";
            foreach (string serverIp in serverIps)
            {
                sb.Append(split);
                var ip = Regex.Replace(serverIp, "http(s)?://", "");
                sb.Append(ip.Replace(':', '_'));
                split = "-";
            }
            return sb.ToString();
        }

        #region 从特定节点获取Nacos服务器列表

        /// <summary>
        /// 只有当EndPoint填写后内部代码有效
        /// </summary>
        private void InitRefreshSrvIfNeed()
        {
            if (string.IsNullOrEmpty(_endpoint))
            {
                return;
            }

            Timer t = new Timer(async x =>
            {
                await RefreshSrvIfNeed();
            }, null, _vipSrvRefInterMillis, _vipSrvRefInterMillis);

            RefreshSrvIfNeed().Wait();
        }

        /// <summary>
        /// 定时刷新服务地址列表
        /// </summary>
        private async Task RefreshSrvIfNeed()
        {
            try
            {
                if (_serverList != null && _serverList.Count > 0)
                {
                    _logger.Debug("server list provided by user");
                    return;
                }

                if (DateTime.Now.GetTimeStamp() - _lastSrvRefTime < _vipSrvRefInterMillis)
                {
                    return;
                }

                var list = await GetServerListFromEndpoint();

                if (list == null || list.Count <= 0)
                {
                    throw new Exception("Can not acquire Nacos list");
                }

                _serversFromEndpoint = list;
                _lastSrvRefTime = DateTime.Now.GetTimeStamp();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "failed to update server list");
            }
        }

        public async Task<IList<string>> GetServerListFromEndpoint()
        {
            string urlString = $"{_endpoint}/nacos/serverlist";
            var headers = BuilderHeaders(null);

            try
            {
                string result = await _http.Request(urlString, headers, null, Encoding.UTF8, HttpMethod.Get);
                var services = new List<string>();
                using (StringReader sr = new StringReader(result))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (line == null || line.Length <= 0)
                            break;

                        services.Add(line.Trim());
                    }
                }

                return services;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while requesting: {urlString} . ");
            }

            return null;
        }

        #endregion

        private Task<string> ReqApi(string api, Dictionary<string, string> headers, Dictionary<string, string> paramValue, HttpMethod httpMethod)
        {
            var snapshot = _serversFromEndpoint;
            if (_serverList != null && _serverList.Count > 0)
            {
                snapshot = _serverList;
            }

            return ReqApi(api, headers, paramValue, snapshot, httpMethod);
        }

        private Task<string> ReqApi(string api, Dictionary<string, string> headers, Dictionary<string, string> param, IList<string> servers, HttpMethod method)
        {
            if (servers?.Count <= 0 && string.IsNullOrEmpty(_nacosDomain))
            {
                throw new ArgumentNullException("no server available");
            }

            Exception exception = new Exception();

            if (servers != null && servers.Count > 0)
            {
                Random random = new Random();
                int index = random.Next(0, servers.Count);

                for (int i = 0; i < servers.Count; i++)
                {
                    string server = servers[index];
                    try
                    {
                        return CallServer(api, headers, param, server, method);
                    }
                    catch (NacosException ex)
                    {
                        exception = ex;
                        _logger.Error(ex, $"request {server} failed.");
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        _logger.Error(ex, $"request {server} failed.");
                    }
                }
                throw new InvalidOperationException($"failed to req API:{api} after all servers {servers} tried: {exception.Message}");
            }

            for (int i = 0; i < REQUEST_DOMAIN_RETRY_COUNT; i++)
            {
                try
                {
                    return CallServer(api, headers, param, _nacosDomain, HttpMethod.Get);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    _logger.Error(ex, $"[NA] req api:{api} failed, server({_nacosDomain}");
                }
            }

            throw new InvalidOperationException($"failed to req API:/api/{api} after all servers({servers}) tried:{exception.Message}");
        }

        private async Task<string> CallServer(string api, Dictionary<string, string> headers, Dictionary<string, string> param, string curServer, HttpMethod method)
        {
            long start = DateTime.Now.GetTimeStamp();
            long end = 0;

            headers = BuilderHeaders(headers);

            string url = curServer + api;

            var result = await _http.Request(url, headers, param, Encoding.UTF8, method);
            end = DateTime.Now.GetTimeStamp();

            return result;
        }

        private Dictionary<string, string> BuilderHeaders(Dictionary<string, string> addheader)
        {
            var headers = new Dictionary<string, string>();
            if (addheader != null)
            {
                headers = addheader;
            }
            headers.Add("Client-Version", VERSION);
            headers.Add("User-Agent", VERSION);
            headers.Add("RequestId", Guid.NewGuid().ToString());
            headers.Add("Request-Module", "Naming");

            return headers;
        }

        public Task<string> Get(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues)
        {
            return ReqApi(url, headers, paramValues, HttpMethod.Get);
        }

        public Task<string> Post(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues)
        {
            return ReqApi(url, headers, paramValues, HttpMethod.Post);
        }

        public Task<string> Delete(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues)
        {
            return ReqApi(url, headers, paramValues, HttpMethod.Delete);
        }

        public string GetName()
        {
            return _name;
        }

        public string GetNamespace()
        {
            return _config.Namespace;
        }

        public string GetTenant()
        {
            return _tenant;
        }
    }
}
