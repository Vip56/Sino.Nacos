﻿using System;

namespace Sino.Nacos.Naming.Exceptions
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
            :base(errMsg)
        {
            ErrorCode = errCode;
            ErrorMsg = errMsg;
        }

        public NacosException(int errCode, Exception ex)
            :base("", ex)
        {
            ErrorCode = errCode;
        }

        public NacosException(int errCode, string errMsg, Exception ex)
            :base(errMsg, ex)
        {
            ErrorCode = errCode;
            ErrorMsg = errMsg;
        }

        public override string ToString()
        {
            return $"ErrCode:{ErrorCode},ErrMsg:{ErrorMsg}";
        }
    }
}
