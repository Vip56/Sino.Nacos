﻿using Newtonsoft.Json;
using NLog;
using Sino.Nacos.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sino.Nacos.Naming.Net
{
    public class FastHttp
    {
        private IHttpClientFactory _httpClientFactory;
        private HttpConfig _httpConfig;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FastHttp(IHttpClientFactory httpClientFactory, HttpConfig config)
        {
            this._httpClientFactory = httpClientFactory;
            this._httpConfig = config;
        }

        public async Task<string> Request(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues, Encoding encoding, HttpMethod method)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMilliseconds(_httpConfig.ConnectionTimeout);

            HttpRequestMessage requestMessage = new HttpRequestMessage();
            requestMessage.Method = method;
            requestMessage.Headers.Connection.Add("Keep-Alive");
            if (paramValues == null || paramValues.Count <= 0)
            {
                requestMessage.RequestUri = new Uri(url);
            }
            else
            {
                var query = HttpUtility.ParseQueryString(string.Empty, encoding);
                foreach(var param in paramValues)
                {
                    query.Add(param.Key, param.Value);
                }
                requestMessage.RequestUri = new Uri(url + "?" + query.ToString());
            }

            SetHeaders(requestMessage.Headers, headers);
            

            // 根据实际调试情况可决定是否需要，主要防止post和put无body出错
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                requestMessage.Content = new FormUrlEncodedContent(paramValues);
            }

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
            catch(Exception ex)
            {
                _logger.Error(ex, "[NA] failed to request" + requestMessage.RequestUri.ToString());
                throw ex;
            }
        }

        private void SetHeaders(HttpRequestHeaders headers, Dictionary<string, string> newHeaders)
        {
            if (newHeaders != null)
            {
                foreach(var header in headers)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            headers.Add("sino", "nacos");
        }
    }
}
