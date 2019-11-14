using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Sino.Nacos.Config.Utils
{
    public static class CryptoUtils
    {
        /// <summary>
        /// 获取MD5
        /// </summary>
        public static string GetMD5String(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
