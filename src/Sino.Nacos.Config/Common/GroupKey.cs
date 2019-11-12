using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Common
{
    public class GroupKey
    {
        public static string GetKey(string dataId, string group)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append('+');
            UrlEncode(group, sb);
            return sb.ToString();
        }

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

        public static string[] ParseKey(string groupKey)
        {
            StringBuilder sb = new StringBuilder();
            string dataId = null;
            string group = null;
            string tenant = null;

            for(int i = 0; i < groupKey.Length; i++)
            {
                char ch = groupKey.ToCharArray()[i];
                if ('+' == ch)
                {
                    if (null == dataId)
                    {
                        dataId = sb.ToString();
                        sb.Clear();
                    }
                    else if (null == group)
                    {
                        group = sb.ToString();
                        sb.Clear();
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid groupKey:{groupKey}");
                    }
                }
                else if('%' == ch)
                {
                    char next = groupKey.ToCharArray()[++i];
                    char nextnext = groupKey.ToCharArray()[++i];
                    if ('2' == next && 'B' == nextnext)
                    {
                        sb.Append('+');
                    }
                    else if('2' == next && '5' == nextnext)
                    {
                        sb.Append('%');
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid groupKey:{groupKey}");
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
                    throw new ArgumentException($"Invalid groupKey:{groupKey}");
                }
            }
            else
            {
                tenant = sb.ToString();
                if (group.Length == 0)
                {
                    throw new ArgumentException($"Invalid groupKey:{groupKey}");
                }
            }

            return new string[] { dataId, group, tenant };
        }

        static void UrlEncode(string str, StringBuilder sb)
        {
            foreach(char ch in str.ToCharArray())
            {
                if ('+' == ch)
                {
                    sb.Append("%2B");
                }
                else if ('%' == ch)
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
