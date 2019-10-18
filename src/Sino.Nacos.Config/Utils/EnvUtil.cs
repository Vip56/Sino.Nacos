using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;

namespace Sino.Nacos.Config.Utils
{
    public class EnvUtil
    {
        private const string AMORY_TAG = "Amory-Tag";
        private const string VIPSERVER_TAG = "Vipserver-Tag";
        private const string LOCATION_TAG = "Location-Tag";

        static Logger _logger = LogManager.GetCurrentClassLogger();

        public static string SelfAmorayTag { get; private set; }
        public static string SelfVipserverTag { get; private set; }
        public static string SelfLocationTag { get; private set; }

        public static void SetSelfEnv(HttpResponseHeaders headers)
        {
            if (headers != null)
            {
                var amorayTagTmp = headers.GetValues(AMORY_TAG);
                if (amorayTagTmp == null)
                {
                    if (!string.IsNullOrEmpty(SelfAmorayTag))
                    {
                        SelfAmorayTag = string.Empty;
                        _logger.Warn("SelfAmorayTag:null");
                    }
                }
                else
                {
                    string amorayTagTmpStr = ListToString(amorayTagTmp);
                    if (!amorayTagTmpStr.Equals(SelfAmorayTag))
                    {
                        SelfAmorayTag = amorayTagTmpStr;
                        _logger.Warn($"SelfAmoryTag:{SelfAmorayTag}");
                    }
                }

                var vipserverTagTmp = headers.GetValues(VIPSERVER_TAG);
                if (vipserverTagTmp == null)
                {
                    if (!string.IsNullOrEmpty(SelfVipserverTag))
                    {
                        SelfVipserverTag = string.Empty;
                        _logger.Warn("SelfVipserverTag:null");
                    }
                }
                else
                {
                    string vipserverTagTmpStr = ListToString(vipserverTagTmp);
                    if (!vipserverTagTmpStr.Equals(SelfVipserverTag))
                    {
                        SelfVipserverTag = vipserverTagTmpStr;
                        _logger.Warn($"SelfVipserverTag:{SelfVipserverTag}");
                    }
                }

                var locationTagTmp = headers.GetValues(LOCATION_TAG);
                if (locationTagTmp == null)
                {
                    if (!string.IsNullOrEmpty(SelfLocationTag))
                    {
                        SelfLocationTag = string.Empty;
                        _logger.Warn("SelfLocationTag:null");
                    }
                }
                else
                {
                    string locationTagTmpStr = ListToString(locationTagTmp);
                    if (!locationTagTmpStr.Equals(SelfLocationTag))
                    {
                        SelfLocationTag = locationTagTmpStr;
                        _logger.Warn($"SelfLocationTag:{SelfLocationTag}");
                    }
                }
            }
        }

        private static string ListToString(IEnumerable<string> list)
        {
            if (list == null || list.Count() <= 0)
            {
                return null;
            }

            StringBuilder result = new StringBuilder();
            foreach(string str in list)
            {
                result.Append(str);
                result.Append(",");
            }
            return result.ToString().Substring(0, result.Length - 1);
        }
    }
}
