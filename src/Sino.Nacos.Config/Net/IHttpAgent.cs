using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sino.Nacos.Config.Net
{
    public interface IHttpAgent
    {
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        Task<string> Get(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues);

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        Task<string> Post(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues);

        /// <summary>
        /// Delete请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        Task<string> Delete(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues);

        string GetName();

        string GetNamespace();

        string GetTenant();
    }
}
