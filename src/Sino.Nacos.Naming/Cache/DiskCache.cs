using Newtonsoft.Json;
using NLog;
using Sino.Nacos.Naming.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Sino.Nacos.Naming.Cache
{
    public class DiskCache
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public const string LINE_SEPARATOR = "&#10;";

        public static void WriteFile(string path, string content)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 1024, false))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[NA] failed to write cache for file: {path}");
            }
        }

        public static void WriteServiceInfo(string dir, ServiceInfo info)
        {
            try
            {
                MakeSureCacheDirExists(dir);

                string fileName = info.GetKeyEncoded();
                string content = JsonConvert.SerializeObject(info);

                WriteFile(Path.Combine(dir, fileName), content);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[NA] failed to write cache for dom: {info.Name}");
            }
        }

        public static string GetLineSeparator()
        {
            return LINE_SEPARATOR;
        }

        public static bool IsFileExists(string path)
        {
            return File.Exists(path);
        }

        public static long GetFileLastModifiedTime(string path)
        {
            return File.GetLastWriteTime(path).GetTimeStamp();
        }

        public static Dictionary<string, ServiceInfo> GetServiceInfos(string dir)
        {
            var infos = new Dictionary<string, ServiceInfo>();

            try
            {
                var files = MakeSureCacheDirExists(dir);

                foreach(string filePath in files)
                {
                    string fileName = HttpUtility.UrlDecode(filePath);

                    if(!(fileName.EndsWith(Constants.SERVICE_INFO_SPLITER + "meta") || fileName.EndsWith(Constants.SERVICE_INFO_SPLITER + "special-url")))
                    {
                        string content = ReadFile(filePath);
                        var info = JsonConvert.DeserializeObject<ServiceInfo>(content);
                        infos.Add(info.GetKey(), info);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"[NA] failed to read cache file");
            }

            return infos;
        }

        public static string ReadFile(string path)
        {
            try
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
            catch(Exception ex)
            {
                _logger.Error(ex, $"[NA] failed to read cache for file: {path}");
            }
            return string.Empty;
        }

        public static string[] MakeSureCacheDirExists(string dir)
        {
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Directory.GetFiles(dir);
        }
    }
}
