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
        private bool _isSnapshot = true;

        public LocalConfigInfoProcessor(ConfigParam config)
        {
            if (string.IsNullOrEmpty(config.LocalFileRoot))
                throw new ArgumentNullException(nameof(config.LocalFileRoot));

            _localFileRootPath = Path.Combine(config.LocalFileRoot, "/nacos/config");
            _localSnapshotPath = Path.Combine(config.LocalFileRoot, "/nacos/config");
        }

        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool IsSnapshot
        {
            get
            {
                return _isSnapshot;
            }
            set
            {
                _isSnapshot = value;
                CleanAllSnapshot();
            }
        }

        /// <summary>
        /// 获取灾备文件
        /// </summary>
        public string GetFailover(string serverName, string dataId, string group, string tenant)
        {
            string file = GetFailoverFile(serverName, dataId, group, tenant);
            if (!File.Exists(file))
            {
                return string.Empty;
            }

            try
            {
                return ReadFile(file);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[{serverName}] get failover error, {file}");
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public string GetSnapshot(string envName, string dataId, string group, string tenant)
        {
            if (!IsSnapshot)
            {
                return string.Empty;
            }

            string file = GetSnapshotPath(envName, dataId, group, tenant);

            if (!File.Exists(file))
            {
                return string.Empty;
            }

            try
            {
                return ReadFile(file);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[{envName}] get snapshot error, {file}");
            }

            return string.Empty;
        }

        /// <summary>
        /// 保存缓存
        /// </summary>
        public void SaveSnapshot(string envName, string dataId, string group, string tenant, string config)
        {
            if (!IsSnapshot)
            {
                return;
            }

            string file = GetSnapshotPath(envName, dataId, group, tenant);

            if (string.IsNullOrEmpty(config))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"[{envName}] delete snapshot error, {file}");
                }
            }
            else
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    if (!info.Directory.Exists)
                    {
                        info.Directory.Create();
                    }

                    WriteFile(file, config);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, $"[{envName}] save snapshot error, {file}");
                }
            }
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

        private void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 1024, false))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }

        private string ReadFile(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024, false))
            {
                byte[] readByte = new byte[fs.Length];
                fs.Read(readByte, 0, readByte.Length);
                string readStr = Encoding.UTF8.GetString(readByte);
                fs.Close();
                return readStr;
            }
        }
    }
}
