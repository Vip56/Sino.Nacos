using NLog;
using Sino.Nacos.Naming.Cache;
using Sino.Nacos.Naming.Core;
using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using Sino.Nacos.Naming.Model;
using Newtonsoft.Json;

namespace Sino.Nacos.Naming.Backups
{
    /// <summary>
    /// 提供灾备支持
    /// </summary>
    /// <remarks>
    /// 如果需要使用该功能需要提前在cacheDir目录下的failover文件夹下新建UtilAndComs.FAILOVER_SWITCH
    /// 指定的文件，并在其中通过写入1和0决定是否开启灾备，一旦开启后获取服务信息将通过灾备存储的信息
    /// 进行访问。该文件需要开发者自行通过其他相关技术进行写入控制从而控制。主要用于Nacos出现重大故障
    /// 无法恢复时可以通过该方式。
    /// </remarks>
    public class FailoverReactor
    {
        public static int SWITCH_REFRESHER_DUETIME = 0;
        public static int SWITCH_REFRESHER_PERIOD = 5000;
        public static int DISK_FILE_WRITER_DUETIME = 30 * 60 * 1000;
        public static int DISK_FILE_WRITER_PERIOD = 24 * 60 * 60 * 1000;
        public static int DIR_NOT_FOUND_DUETIME = 10 * 1000;
        public static string FAILOVER_MODE_NAME = "failover-mode";
        public static string FAILOVER_PATH = "failover";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _failoverDir;
        private IHostReactor _hostReactor;

        private Timer _switchRefresher;
        private Timer _diskFileWriter;

        private ConcurrentDictionary<string, string> _switchParams = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, ServiceInfo> _serviceMap = new ConcurrentDictionary<string, ServiceInfo>();

        private long _failoverLastModifiedMillis = 0;

        public FailoverReactor(IHostReactor hostReactor, string cacheDir)
        {
            _hostReactor = hostReactor;
            _failoverDir = Path.Combine(cacheDir, FAILOVER_PATH);

            Init();
        }

        public void Init()
        {
            SwitchRefresher();
            DiskFileWriter();
        }

        /// <summary>
        /// 定时刷新灾备状态，主要根据灾备目录下对应文件的内容
        /// </summary>
        private void SwitchRefresher()
        {
            _switchRefresher = new Timer(x =>
            {
                try
                {
                    string filePath = Path.Combine(_failoverDir, UtilAndComs.FAILOVER_SWITCH);
                    if (!DiskCache.IsFileExists(filePath))
                    {
                        _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
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
                                    _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "true", (k, v) => "true");
                                    _logger.Info($"{FAILOVER_MODE_NAME} is on");
                                    FailoverFileReader();
                                }
                                else if ("0".Equals(line.Trim()))
                                {
                                    _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
                                    _logger.Info($"{FAILOVER_MODE_NAME} is off");
                                }
                            }
                        }
                        else
                        {
                            _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
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

        /// <summary>
        /// 创建服务信息刷新定时任务
        /// </summary>
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

            // 定时将新的服务信息保存至灾备目录文件中
            _diskFileWriter = new Timer(x =>
            {
                var map = _hostReactor.GetServiceInfoMap();
                foreach (var entry in map)
                {
                    ServiceInfo serviceInfo = entry.Value;

                    // 跳过系统配置
                    if (serviceInfo.GetKey().Equals(UtilAndComs.ALL_IPS) || serviceInfo.Name.Equals(UtilAndComs.ENV_LIST_KEY)
                     || serviceInfo.Name.Equals(UtilAndComs.ENV_CONFIGS)
                     || serviceInfo.Name.Equals(UtilAndComs.VIPCLIENT_CONFIG)
                     || serviceInfo.Name.Equals(UtilAndComs.ALL_HOSTS))
                    {
                        continue;
                    }

                    DiskCache.WriteServiceInfo(_failoverDir, serviceInfo);
                }
                _diskFileWriter.Change(DISK_FILE_WRITER_PERIOD, Timeout.Infinite);
            }, null, duetime, Timeout.Infinite);
        }

        /// <summary>
        /// 通过灾备目录中的服务信息恢复
        /// </summary>
        private void FailoverFileReader()
        {
            var domMap = new ConcurrentDictionary<string, ServiceInfo>();
            try
            {
                var files = DiskCache.MakeSureCacheDirExists(_failoverDir);

                foreach(var filePath in files)
                {
                    var fi = new FileInfo(filePath);

                    // 跳过灾备状态文件
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

        /// <summary>
        /// 当前灾备是否处于打开状态
        /// </summary>
        public bool IsFailoverSwitch()
        {
            string failover = "false";
            if(_switchParams.TryGetValue(FAILOVER_MODE_NAME, out failover))
            {
                return failover.Equals("false") ? false : true;
            }
            return false;
        }

        /// <summary>
        /// 通过灾备存储的数据获取服务
        /// </summary>
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
