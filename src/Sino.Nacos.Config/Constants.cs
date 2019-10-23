using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config
{
    public class Constants
    {
        public const string DEFAULT_GROUP = "DEFAULT_GROUP";
        public const string DEFAULT_TENANT_ID = "nacosconfig";

        public const string BASE_PATH = "/v1/cs";
        public const string CONFIG_CONTROLLER_PATH = BASE_PATH + "/configs";
    }
}
