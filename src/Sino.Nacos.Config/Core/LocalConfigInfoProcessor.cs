using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sino.Nacos.Config.Core
{
    public class LocalConfigInfoProcessor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _localFileRootPath;
        private string _localSnapshotPath;

        public LocalConfigInfoProcessor(ConfigParam config)
        {
            if (string.IsNullOrEmpty(config.LocalFileRoot))
                throw new ArgumentNullException(nameof(config.LocalFileRoot));

            _localFileRootPath = Path.Combine(config.LocalFileRoot, "/nacos/config");
            _localSnapshotPath = Path.Combine(config.LocalFileRoot, "/nacos/config");
        }

        /// <summary>
        /// 清除所有缓存文件
        /// </summary>
        public void CleanAllSnapshot()
        {
            try
            {
                var files = Directory.GetDirectories(_localSnapshotPath);
                if (files == null || files.Length <= 0)
                {
                    return;
                }

                foreach(var file in files)
                {
                    if (file.EndsWith("_nacos"))
                    {
                        Directory.Delete(file, true);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "clean all snapshot error.");
            }
        }

        /// <summary>
        /// 根据环境清除缓存文件
        /// </summary>
        public void CleanAllSnapshot(string envName)
        {
            string path = Path.Combine(_localSnapshotPath, envName, "_nacos", "snapshot");
            try
            {
                Directory.Delete(path, true);
                _logger.Info($"success delete {envName} -snapshot");
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"fail delete {envName}-snapshot");
            }
        }

        /// <summary>
        /// 获取灾备路径
        /// </summary>
        private string GetFailoverFile(string serverName, string dataId, string group, string tenant)
        {
            string tmp = Path.Combine(_localSnapshotPath, serverName, "_nacos", "data");
            if (string.IsNullOrEmpty(tenant))
            {
                tmp = Path.Combine(tmp, "config-data");
            }
            else
            {
                tmp = Path.Combine(tmp, "config-data-tenant", tenant);
            }
            return Path.Combine(tmp, group, dataId);
        }

        /// <summary>
        /// 获取缓存路径
        /// </summary>
        private string GetSnapshotPath(string envName, string dataId, string group, string tenant)
        {
            string tmp = Path.Combine(_localSnapshotPath, envName, "_nacos");
            if (string.IsNullOrEmpty(tenant))
            {
                tmp = Path.Combine(tmp, "snapshot");
            }
            else
            {
                tmp = Path.Combine(tmp, "snapshot-tenant", tenant);
            }

            return Path.Combine(tmp, group, dataId);
        }
    }
}
