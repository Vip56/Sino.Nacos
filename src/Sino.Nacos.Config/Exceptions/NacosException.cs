using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Exceptions
{
    /// <summary>
    /// 异常
    /// </summary>
    public class NacosException : Exception
    {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }

        public NacosException() { }

        public NacosException(int errCode, string errMsg)
            : base(errMsg)
        {
            ErrorCode = errCode;
            ErrorMsg = errMsg;
        }

        public NacosException(int errCode, Exception ex)
            : base("", ex)
        {
            ErrorCode = errCode;
        }

        public NacosException(int errCode, string errMsg, Exception ex)
            : base(errMsg, ex)
        {
            ErrorCode = errCode;
            ErrorMsg = errMsg;
        }

        public override string ToString()
        {
            return $"ErrCode:{ErrorCode},ErrMsg:{ErrorMsg}";
        }

        /// <summary>
        /// 参数错误
        /// </summary>
        public const int CLIENT_INVALID_PARAM = -400;

        /// <summary>
        /// 超过Server端的限流阈值
        /// </summary>
        public const int CLIENT_OVER_THRESHOLD = -503;

        /// <summary>
        /// 参数错误
        /// </summary>
        public const int INVALID_PARAM = 400;

        /// <summary>
        /// 鉴权失败
        /// </summary>
        public const int NO_RIGHT = 403;

        /// <summary>
        /// 写并发冲突
        /// </summary>
        public const int CONFLICT = 409;
    }
}
