using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config
{
    public class Constants
    {
        public const string DEFAULT_GROUP = "DEFAULT_GROUP";
        public const string DEFAULT_TENANT_ID = "nacosconfig";

        public const string BASE_PATH = "/nacos/v1/cs";
        public const string CONFIG_CONTROLLER_PATH = BASE_PATH + "/configs";
        public const string PROBE_MODIFY_REQUEST = "Listening-Configs";

        public const char LINE_SEPARATOR = (char)1;
        public const char WORD_SEPARATOR = (char)2;
    }
}
