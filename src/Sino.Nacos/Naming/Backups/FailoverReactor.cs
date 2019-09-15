using NLog;
using Sino.Nacos.Naming.Cache;
using Sino.Nacos.Naming.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using Sino.Nacos.Naming.Model;
using Newtonsoft.Json;

namespace Sino.Nacos.Naming.Backups
{
    /// <summary>
    /// 定时备份并提供灾备
    /// </summary>
    public class FailoverReactor
    {
        public const int SWITCH_REFRESHER_DUETIME = 0;
        public const int SWITCH_REFRESHER_PERIOD = 5000;
        public const int DISK_FILE_WRITER_DUETIME = 30 * 60 * 1000;
        public const int DISK_FILE_WRITER_PERIOD = 24 * 60 * 60 * 1000;
        public const int DIR_NOT_FOUND_DUETIME = 10 * 1000;

        private string _failoverDir;
        private HostReactor _hostReactor;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private Timer _switchRefresher;
        private Timer _diskFileWriter;

        private ConcurrentDictionary<string, string> _switchParams = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, ServiceInfo> _serviceMap = new ConcurrentDictionary<string, ServiceInfo>();

        private long _failoverLastModifiedMillis = 0;

        public FailoverReactor(HostReactor hostReactor, string cacheDir)
        {
            _hostReactor = hostReactor;
            _failoverDir = Path.Combine(cacheDir, "failover");

            Init();
        }

        public void Init()
        {
            SwitchRefresher();
            DiskFileWriter();
        }

        private void SwitchRefresher()
        {
            _switchRefresher = new Timer(x =>
            {
                try
                {
                    string filePath = Path.Combine(_failoverDir, UtilAndComs.FAILOVER_SWITCH);
                    if (!DiskCache.IsFileExists(filePath))
                    {
                        _switchParams.AddOrUpdate("failover-mode", "false", (k, v) => "false");
                        _logger.Debug($"failover switch is not found, {filePath}");
                        return;
                    }

                    long modified = DiskCache.GetFileLastModifiedTime(filePath);
                    if (_failoverLastModifiedMillis < modified)
                    {
                        _failoverLastModifiedMillis = modified;
                        string failover = DiskCache.ReadFile(filePath);

                        if (!string.IsNullOrEmpty(failover))
                        {
                            var lines = failover.Split(new string[] { DiskCache.GetLineSeparator() }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                            {
                                if ("1".Equals(line.Trim()))
                                {
                                    _switchParams.AddOrUpdate("failover-mode", "true", (k, v) => "true");
                                    _logger.Info("failover-mode is on");
                                    FailoverFileReader();
                                }
                                else if ("0".Equals(line.Trim()))
                                {
                                    _switchParams.AddOrUpdate("failover-mode", "false", (k, v) => "false");
                                    _logger.Info("failover-mode is off");
                                }
                            }
                        }
                        else
                        {
                            _switchParams.AddOrUpdate("failover-mode", "false", (k, v) => "false");
                        }
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "[NA] failed to read failover switch.");
                }

                _switchRefresher.Change(SWITCH_REFRESHER_PERIOD, Timeout.Infinite);
            }, null, SWITCH_REFRESHER_DUETIME, Timeout.Infinite);
        }

        private void DiskFileWriter()
        {
            int duetime = DISK_FILE_WRITER_DUETIME;
            try
            {
                var files = DiskCache.MakeSureCacheDirExists(_failoverDir);
                if (files == null || files.Length <= 0)
                {
                    duetime = DIR_NOT_FOUND_DUETIME;
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "[NA] failed to backup file on startup.");
            }

            _diskFileWriter = new Timer(x =>
            {
                var map = _hostReactor.GetServiceInfoMap();
                foreach (var entry in map)
                {
                    ServiceInfo serviceInfo = entry.Value;
                    if (serviceInfo.GetKey().Equals(UtilAndComs.ALL_IPS) || serviceInfo.Name.Equals(UtilAndComs.ENV_LIST_KEY)
                     || serviceInfo.Name.Equals("00-00---000-ENV_CONFIGS-000---00-00")
                     || serviceInfo.Name.Equals("vipclient.properties")
                     || serviceInfo.Name.Equals("00-00---000-ALL_HOSTS-000---00-00"))
                    {
                        continue;
                    }

                    DiskCache.WriteServiceInfo(_failoverDir, serviceInfo);
                }
                _diskFileWriter.Change(DISK_FILE_WRITER_PERIOD, Timeout.Infinite);
            }, null, duetime, Timeout.Infinite);
        }

        private void FailoverFileReader()
        {
            var domMap = new ConcurrentDictionary<string, ServiceInfo>();
            try
            {
                var files = DiskCache.MakeSureCacheDirExists(_failoverDir);

                foreach(var filePath in files)
                {
                    var fi = new FileInfo(filePath);

                    if (fi.Name.Equals(UtilAndComs.FAILOVER_SWITCH))
                    {
                        continue;
                    }

                    string content = DiskCache.ReadFile(filePath);
                    ServiceInfo serviceInfo = JsonConvert.DeserializeObject<ServiceInfo>(content);
                    if (serviceInfo.Hosts != null && serviceInfo.Hosts.Count > 0)
                    {
                        domMap.AddOrUpdate(serviceInfo.GetKey(), serviceInfo, (k, v) => serviceInfo);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "[NA] failed to read cache files");
            }

            if (domMap.Count > 0)
            {
                _serviceMap = domMap;
            }
        }

        public bool IsFailoverSwitch()
        {
            string failover = "false";
            _switchParams.TryGetValue("failover-mode", out failover);
            return failover.Equals("false") ? false : true;
        }

        public ServiceInfo GetService(string key)
        {
            ServiceInfo serviceInfo = null;
            if (!_serviceMap.TryGetValue(key, out serviceInfo))
            {
                serviceInfo = new ServiceInfo();
                serviceInfo.Name = key;
            }
            return serviceInfo;
        }
    }
}
