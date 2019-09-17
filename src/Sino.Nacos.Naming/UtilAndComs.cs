using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming
{
    public class UtilAndComs
    {
        public const string VERSION = "Nacos-Java-Client:v1.1.3";

        public const string WEB_CONTEXT = "/nacos";

        public const string NACOS_URL_BASE = WEB_CONTEXT + "/v1/ns";

        public const string NACOS_URL_INSTANCE = NACOS_URL_BASE + "/instance";

        public const string NACOS_URL_SERVICE = NACOS_URL_BASE + "/service";

        public const string FAILOVER_SWITCH = "00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00";
        public const string ALL_IPS = "000--00-ALL_IPS--00--000";
        public const string ENV_CONFIGS = "00-00---000-ENV_CONFIGS-000---00-00";
        public const string VIPCLIENT_CONFIG = "vipclient.properties";
        public const string ALL_HOSTS = "00-00---000-ALL_HOSTS-000---00-00";

        public const string ENV_LIST_KEY = "envList";
    }
}
