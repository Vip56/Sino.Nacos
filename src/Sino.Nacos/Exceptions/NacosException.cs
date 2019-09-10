using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Exceptions
{
    /// <summary>
    /// 异常
    /// </summary>
    public class NacosException : Exception
    {
        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }

        public NacosException() { }

        public NacosException(int errCode, string errMsg)
            :base(errMsg)
        {
            ErrCode = errCode;
            ErrMsg = errMsg;
        }

        public NacosException(int errCode, Exception ex)
            :base("", ex)
        {
            ErrCode = errCode;
        }

        public NacosException(int errCode, string errMsg, Exception ex)
            :base(errMsg, ex)
        {
            ErrCode = errCode;
            ErrMsg = errMsg;
        }

        public override string ToString()
        {
            return $"ErrCode:{ErrCode},ErrMsg:{ErrMsg}";
        }
    }
}
