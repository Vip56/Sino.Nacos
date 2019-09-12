﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Common
{
    public class Constants
    {
        public const string CLIENT_VERSION = "3.0.0";
        public const int DATA_IN_BODY_VERSION = 204;
        public const string DEFAULT_GROUP = "DEFAULT_GROUP";
        public const string APPNAME = "AppName";
        public const string UNKNOWN_APP = "UnknownApp";
        public const string DEFAULT_DOMAINNAME = "commonconfig.config-host.taobao.com";
        public const string DAILY_DOMAINNAME = "commonconfig.taobao.net";
        public const string NULL = "";
        public const long DEFAULT_HEART_BEAT_INTERVAL = 5 * 60 * 1000;
        public const long DEFAULT_IP_DELETE_TIMEOUT = 30 * 60 * 1000;
        public const long DEFAULT_HEART_BEAT_TIMEOUT = 15 * 60 * 1000;
        public const char SERVICE_INFO_SPLITER = '@';
    }
}
