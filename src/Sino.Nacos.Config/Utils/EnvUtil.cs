using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Sino.Nacos.Config.Utils
{
    public class EnvUtil
    {
        private const string AMORY_TAG = "Amory-Tag";
        private const string VIPSERVER_TAG = "Vipserver-Tag";
        private const string LOCATION_TAG = "Location-Tag";

        static Logger _logger = LogManager.GetCurrentClassLogger();

        public string SelfAmorayTag { get; private set; }
        public string SelfVipserverTag { get; private set; }
        public string SelfLocationTag { get; private set; }

        public static void SetSelfEnv(HttpResponseHeaders headers)
        {
            if (headers != null)
            {

            }
        }

        private static string ListToString(IList<string> list)
        {
            if (list == null || list.Count <= 0)
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
