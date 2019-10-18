using NLog;
using Sino.Nacos.Config.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sino.Nacos.Config.Net
{
    /// <summary>
    /// 提供Http请求支持
    /// </summary>
    public class FastHttp
    {
        private IHttpClientFactory _httpClientFactory;
        private ConfigParam _config;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FastHttp(IHttpClientFactory httpClientFactory, ConfigParam config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> Request(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues, Encoding encoding, HttpMethod method)
        {
            var client = _httpClientFactory.CreateClient();

            if (client.Timeout.TotalMilliseconds != _config.ConnectionTimeout)
                client.Timeout = TimeSpan.FromMilliseconds(_config.ConnectionTimeout);

            HttpRequestMessage requestMessage = new HttpRequestMessage();
            requestMessage.Method = method;
            if (paramValues == null || paramValues.Count <= 0)
            {
                requestMessage.RequestUri = new Uri(url);
            }
            else
            {
                var query = HttpUtility.ParseQueryString(string.Empty, encoding);
                foreach (var param in paramValues)
                {
                    query.Add(param.Key, param.Value);
                }
                requestMessage.RequestUri = new Uri(url + "?" + query.ToString());
            }

            SetHeaders(requestMessage.Headers, headers);


            // 根据实际调试情况可决定是否需要，主要防止post和put无body出错
            //if (method == HttpMethod.Post || method == HttpMethod.Put)
            //{
            //    requestMessage.Content = new FormUrlEncodedContent(paramValues);
            //}

            try
            {
                var response = await client.SendAsync(requestMessage);
                _logger.Debug($"Request from server {requestMessage.RequestUri.ToString()}");

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    return responseStr;
                }
                else
                {
                    _logger.Warn($"Error while requesting: {requestMessage.RequestUri.ToString()}. Server returned: {response.StatusCode}");
                    throw new NacosException(500, $"failed to req API: {requestMessage.RequestUri.ToString()}. code: {response.StatusCode} msg: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[NA] failed to request" + requestMessage.RequestUri.ToString());
                throw ex;
            }
        }

        private void SetHeaders(HttpRequestHeaders headers, Dictionary<string, string> newHeaders)
        {
            if (newHeaders != null)
            {
                foreach (var header in newHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            headers.Add("sino", "nacos");
        }
    }
}
