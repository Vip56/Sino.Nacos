using System;
using System.Text;

namespace Sino.Nacos.Config.Common
{
    /// <summary>
    /// 将dataId和groupId进行合并，同时转义其中的特殊字符。
    /// </summary>
    public static class GroupKey
    {
        /// <summary>
        /// 获取分组键
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        public static string GetKey(string dataId, string group)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append('+');
            UrlEncode(group, sb);
            return sb.ToString();
        }

        /// <summary>
        /// 获取分组键
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="tenant">命名空间</param>
        public static string GetKeyTenant(string dataId, string group, string tenant)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append('+');
            UrlEncode(group, sb);
            if (!string.IsNullOrEmpty(tenant))
            {
                sb.Append('+');
                UrlEncode(tenant, sb);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取分组键
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="datumStr">数据</param>
        public static string GetKey(string dataId, string group, string datumStr)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append('+');
            UrlEncode(group, sb);
            sb.Append('+');
            UrlEncode(datumStr, sb);
            return sb.ToString();
        }

        /// <summary>
        /// 解析分组键
        /// </summary>
        public static string[] ParseKey(string groupKey)
        {
            StringBuilder sb = new StringBuilder();
            string dataId = null, group = null, tenant = null;

            var chs = groupKey.ToCharArray();
            for (int i = 0; i < chs.Length; ++i)
            {
                char ch = chs[i];
                if ('+' == ch)
                {
                    if (dataId == null)
                    {
                        dataId = sb.ToString();
                        sb.Clear();
                    } else if (group == null)
                    {
                        group = sb.ToString();
                        sb.Clear();
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid groupKey:" + groupKey);
                    }
                }
                else if('%' == ch)
                {
                    char next = chs[++i];
                    char nextnext = chs[++i];
                    if ('2' == next && 'B' == nextnext)
                    {
                        sb.Append('+');
                    }
                    else if ('2' == next && '5' == nextnext)
                    {
                        sb.Append('%');
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid groupKey:" + groupKey);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            if (string.IsNullOrEmpty(group))
            {
                group = sb.ToString();
                if (group.Length == 0)
                {
                    throw new InvalidOperationException("invalid groupKey:" + groupKey);
                }
            }
            else
            {
                tenant = sb.ToString();
                if (group.Length ==0)
                {
                    throw new InvalidOperationException("invalid groupKey:" + groupKey);
                }
            }

            return new string[] { dataId, group, tenant };
        }

        public static void UrlEncode(string str, StringBuilder sb)
        {
            foreach(var ch in str.ToCharArray())
            {
                if ('+' == ch)
                {
                    sb.Append("%2B");
                }
                else if('%' == ch)
                {
                    sb.Append("%25");
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }
    }
}
