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
        /// 获取Nacos IP列表
        /// </summary>
        void Start();

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        /// <param name="encoding">编码</param>
        /// <param name="readTimeout">请求超时时间，单位毫秒</param>
        Task<string> Get(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues, Encoding encoding, long readTimeout);

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        /// <param name="encoding">编码</param>
        /// <param name="readTimeout">请求超时时间，单位毫秒</param>
        Task<string> Post(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues, Encoding encoding, long readTimeout);

        /// <summary>
        /// Delete请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="headers">请求头部</param>
        /// <param name="paramValues">请求参数</param>
        /// <param name="encoding">编码</param>
        /// <param name="readTimeout">请求超时时间，单位毫秒</param>
        Task<string> Delete(string url, Dictionary<string, string> headers, Dictionary<string, string> paramValues, Encoding encoding, long readTimeout);

        string GetName();

        string GetNamespace();

        string GetTenant();

        string GetEncode();
    }
}
