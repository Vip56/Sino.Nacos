using System;
using System.Net.Http;

namespace NacosNamingUnitTest
{
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        private HttpClient _httpClient;

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }

        public FakeHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static IHttpClientFactory Create(HttpClient httpClient)
        {
            return new FakeHttpClientFactory(httpClient);
        }
    }
}
