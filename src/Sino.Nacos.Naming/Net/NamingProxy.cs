using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NLog;
using Newtonsoft.Json;
using System.IO;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Naming.Utils;
using Sino.Nacos.Naming.Exceptions;

namespace Sino.Nacos.Naming.Net
{
    /// <summary>
    /// 服务注册发现代理
    /// </summary>
    public class NamingProxy
    {
        public const string SERVICE_NAME = "serviceName";
        public const string CLUSTER_NAME = "clusterName";
        public const string NAMESPACE_ID = "namespaceId";
        public const string GROUP_NAME = "groupName";
        public const int REQUEST_DOMAIN_RETRY_COUNT = 3;
        public const string VERSION = "Nacos-Java-Client:v1.1.3";

        private string _namespace;
        private string _endpoint;
        private string _nacosDomain;
        private IList<string> _serverList;

        private NamingConfig _httpConfig;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 通过EndPoint获取到的Nacos服务器地址
        /// </summary>
        private IList<string> _serversFromEndpoint = new List<string>();

        /// <summary>
        /// 最后一次从EndPoint获取Nacos服务器的时间
        /// </summary>
        private long _lastSrvRefTime = 0;

        /// <summary>
        /// 从EndPoint获取最新Nacos服务器列表的响应间隔时间
        /// </summary>
        private int _vipSrvRefInterMillis = 30 * 60 * 1000;

        private FastHttp _http;

        public NamingProxy(NamingConfig config, FastHttp http)
        {
            _httpConfig = config;
            _http = http;
            Init(config);
        }

        public void Init(NamingConfig config)
        {
            _namespace = config.Namespace;
            _endpoint = config.EndPoint;
            _serverList = config.ServerAddr;
            if (_serverList != null || _serverList.Count == 1)
            {
                _nacosDomain = _serverList.First();
            }

            InitRefreshSrvIfNeed().Wait();
        }

        /// <summary>
        /// 只有当EndPoint填写后内部代码有效
        /// </summary>
        private async Task InitRefreshSrvIfNeed()
        {
            if (string.IsNullOrEmpty(_endpoint))
            {
                return;
            }

            Timer t = new Timer(async x =>
            {
                await RefreshSrvIfNeed();
            }, null, 0, _vipSrvRefInterMillis);

            await RefreshSrvIfNeed();
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
            catch(Exception ex)
            {
                _logger.Warn(ex, "failed to update server list");
            }
        }

        public async Task RegisterService(string serviceName, string groupName, Instance instance)
        {
            _logger.Info($"[REGISTER-SERVICE] {_namespace} registering service {serviceName} with instance: {instance}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add(GROUP_NAME, groupName);
            paramValue.Add(CLUSTER_NAME, instance.ClusterName);
            paramValue.Add("ip", instance.Ip);
            paramValue.Add("port", instance.Port.ToString());
            paramValue.Add("weight", instance.Weight.ToString());
            paramValue.Add("enable", instance.Enable.ToString());
            paramValue.Add("healthy", instance.Healthy.ToString());
            paramValue.Add("ephemeral", instance.Ephemeral.ToString());
            paramValue.Add("metadata", JsonConvert.SerializeObject(instance.Metadata));

            await ReqApi<string>(UtilAndComs.NACOS_URL_INSTANCE, paramValue, HttpMethod.Post);
        }

        public async Task DeregisterService(string serviceName, Instance instance)
        {
            _logger.Info($"[DEREGITER-SERVICE] {_namespace} deregistering service {serviceName} with instance: {instance}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add(CLUSTER_NAME, instance.ClusterName);
            paramValue.Add("ip", instance.Ip);
            paramValue.Add("port", instance.Port.ToString());
            paramValue.Add("ephemeral", instance.Ephemeral.ToString());

            await ReqApi<string>(UtilAndComs.NACOS_URL_INSTANCE, paramValue, HttpMethod.Delete);
        }

        public async Task UpdateInstance(string serviceName, string groupName, Instance instance)
        {
            _logger.Info($"[UPDATE-SERVICE] {_namespace} update service {serviceName} with instance: {instance}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add(GROUP_NAME, groupName);
            paramValue.Add(CLUSTER_NAME, instance.ClusterName);
            paramValue.Add("ip", instance.Ip);
            paramValue.Add("port", instance.Port.ToString());
            paramValue.Add("weight", instance.Weight.ToString());
            paramValue.Add("enabled", instance.Enable.ToString());
            paramValue.Add("ephemeral", instance.Ephemeral.ToString());
            paramValue.Add("metadata", JsonConvert.SerializeObject(instance.Metadata));

            await ReqApi<string>(UtilAndComs.NACOS_URL_INSTANCE, paramValue, HttpMethod.Put);
        }

        public async Task<Service> QueryService(string serviceName, string groupName)
        {
            _logger.Info($"[QUERY-SERVICE] {_namespace} query service: {serviceName}, {groupName}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add(GROUP_NAME, groupName);

            return await ReqApi<Service>(UtilAndComs.NACOS_URL_SERVICE, paramValue, HttpMethod.Get);
        }

        public async Task CreateService(Service service, Selector selector)
        {
            _logger.Info($"[CREATE-SERVICE] {_namespace} create service: {service}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, service.Name);
            paramValue.Add(GROUP_NAME, service.GroupName);
            paramValue.Add("protectThreshold", service.ProtectThreshold.ToString());
            paramValue.Add("metadata", JsonConvert.SerializeObject(service.Metadata));
            paramValue.Add("selector", JsonConvert.SerializeObject(selector));

            await ReqApi<string>(UtilAndComs.NACOS_URL_SERVICE, paramValue, HttpMethod.Post);
        }

        public async Task<bool> DeleteService(string serviceName, string groupName)
        {
            _logger.Info($"[DELETE-SERVICE] {_namespace} deleting service: {serviceName} with groupName: {groupName}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add(GROUP_NAME, groupName);

            string result = await ReqApi<string>(UtilAndComs.NACOS_URL_SERVICE, paramValue, HttpMethod.Delete);
            return "ok".Equals(result);
        }

        public async Task UpdateService(Service service, Selector selector)
        {
            _logger.Info($"[UPDATE-SERVICE] {_namespace} updating service : {service}");

            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, service.Name);
            paramValue.Add(GROUP_NAME, service.GroupName);
            paramValue.Add("protectThreshold", service.ProtectThreshold.ToString());
            paramValue.Add("metadata", JsonConvert.SerializeObject(service.Metadata));
            paramValue.Add("selector", JsonConvert.SerializeObject(selector));

            await ReqApi<string>(UtilAndComs.NACOS_URL_SERVICE, paramValue, HttpMethod.Put);
        }

        public async Task<string> QueryList(string serviceName, string clusters, int udpPort, bool healthyOnly)
        {
            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(SERVICE_NAME, serviceName);
            paramValue.Add("clusters", clusters);
            paramValue.Add("udpPort", udpPort.ToString());
            paramValue.Add("clientIP", _httpConfig.Ip);
            paramValue.Add("healthyOnly", healthyOnly.ToString());

            return await ReqApi<string>(UtilAndComs.NACOS_URL_BASE + "/instance/list", paramValue, HttpMethod.Get);
        }

        public async Task<long> SendBeat(BeatInfo beatInfo)
        {
            try
            {
                _logger.Debug($"[BEAT] {_namespace} sending beat to server: {beatInfo}");

                Dictionary<string, string> paramValue = new Dictionary<string, string>();
                paramValue.Add("beat", beatInfo.ToString());
                paramValue.Add(NAMESPACE_ID, _namespace);
                paramValue.Add(SERVICE_NAME, beatInfo.ServiceName);
                string result = await ReqApi<string>(UtilAndComs.NACOS_URL_BASE + "/instance/beat", paramValue, HttpMethod.Put);

                // 这里还需要从结果的Json中获取clientBeatInterval的值返回，但是实际openApi文档中只返回OK
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CLIENT-BEAT] failed to send beat: {beatInfo}");
            }
            return 0L;
        }

        public async Task<bool> ServerHealthy()
        {
            try
            {
                ServiceMetrics metrics = await ReqApi<ServiceMetrics>(UtilAndComs.NACOS_URL_BASE + "/operator/metrics", new Dictionary<string, string>(), HttpMethod.Get);
                return "UP".Equals(metrics.Status);
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<ServiceList> GetServiceList(int pageNo, int pageSize, string groupName, Selector selector)
        {
            Dictionary<string, string> paramValue = new Dictionary<string, string>();
            paramValue.Add("pageNo", pageNo.ToString());
            paramValue.Add("pageSize", pageSize.ToString());
            paramValue.Add(NAMESPACE_ID, _namespace);
            paramValue.Add(GROUP_NAME, groupName);

            //selector暂时不实现

            return await ReqApi<ServiceList>(UtilAndComs.NACOS_URL_BASE + "/service/list", paramValue, HttpMethod.Get);
        }

        public async Task<IList<string>> GetServerListFromEndpoint()
        {
            string urlString = $"http://{_endpoint}/nacos/serverlist";
            var headers = BuilderHeaders();

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

        public async Task<T> CallServer<T>(string api, Dictionary<string, string> param, string curServer, HttpMethod method)
        {
            long start = DateTime.Now.GetTimeStamp();
            long end = 0;

            CheckSignature(param);
            var headers = BuilderHeaders();

            string url;
            if (curServer.StartsWith("https://") || curServer.StartsWith("http://"))
            {
                url = curServer + api;
            }
            else
            {
                if (!curServer.Contains(':'))
                {
                    curServer = curServer + ":" + _httpConfig.Port;
                }
                url = "http://" + curServer + api;
            }

            var result = await _http.Request(url, headers, param, Encoding.UTF8, method);
            end = DateTime.Now.GetTimeStamp();
            return JsonConvert.DeserializeObject<T>(result);
        }

        public Task<T> ReqApi<T>(string api, Dictionary<string, string> paramValue, HttpMethod httpMethod)
        {
            var snapshot = _serversFromEndpoint;
            if (_serverList != null && _serverList.Count > 0)
            {
                snapshot = _serverList;
            }

            return ReqApi<T>(api, paramValue, snapshot, httpMethod);
        }

        public Task<T> ReqApi<T>(string api, Dictionary<string, string> param, IList<string> servers, HttpMethod method)
        {
            param.Add(NAMESPACE_ID, _namespace);

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
                        return CallServer<T>(api, param, server, method);
                    }
                    catch(NacosException ex)
                    {
                        exception = ex;
                        _logger.Error(ex, $"request {server} failed.");
                    }
                    catch(Exception ex)
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
                    return CallServer<T>(api, param, _nacosDomain, HttpMethod.Get);
                }
                catch(Exception ex)
                {
                    exception = ex;
                    _logger.Error(ex, $"[NA] req api:{api} failed, server({_nacosDomain}");
                }
            }

            throw new InvalidOperationException($"failed to req API:/api/{api} after all servers({servers}) tried:{exception.Message}");
        }

        private void CheckSignature(Dictionary<string, string> param)
        {
            param.Add("app", AppDomain.CurrentDomain.FriendlyName);
            if (string.IsNullOrEmpty(_httpConfig.AccessKey) && string.IsNullOrEmpty(_httpConfig.SecretKey))
            {
                return;
            }

            string signData = string.IsNullOrEmpty(_httpConfig.ServiceName) ? DateTime.Now.GetTimeStamp() + "@@" + _httpConfig.ServiceName : DateTime.Now.GetTimeStamp().ToString();
            string signature = Sign(signData, _httpConfig.SecretKey);
            param.Add("signature", signature);
            param.Add("data", signData);
            param.Add("ak", _httpConfig.AccessKey);
        }

        private string Sign(string data, string key)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteData = Encoding.UTF8.GetBytes(data);


            HMACSHA1 hmac = new HMACSHA1(byteKey);
            return Convert.ToBase64String(hmac.ComputeHash(byteData));
        }

        private Dictionary<string, string> BuilderHeaders()
        {
            var headers = new Dictionary<string, string>();
            headers.Add("Client-Version", VERSION);
            headers.Add("User-Agent", VERSION);
            headers.Add("RequestId", Guid.NewGuid().ToString());
            headers.Add("Request-Module", "Naming");

            return headers;
        }
    }
}
