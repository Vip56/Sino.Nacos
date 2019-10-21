using NLog;
using Sino.Nacos.Config.Filter;
using Sino.Nacos.Config.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Core
{
    public class CacheData
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _content;
        private ConcurrentList<ManagerListenerWrap> _listeners;
        private string _name;
        private bool _isUseLocalConfig;
        private ConfigFilterChainManager _configFilterChainManager;

        public string DataId { get; set; }
        public string Group { get; set; }
        public string Tenant { get; set; }

        public bool Initializing { get; set; }

        public string MD5 { get; private set; }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                MD5 = GetMD5String(value);
            }
        }

        /// <summary>
        /// 本地配置最后修改时间戳
        /// </summary>
        public long LocalConfigLastModified { get; set; }

        public int TaskId { get; set; }

        /// <summary>
        /// 是否启用本地配置
        /// </summary>
        public bool IsUseLocalConfig
        {
            get
            {
                return _isUseLocalConfig;
            }
            set
            {
                _isUseLocalConfig = value;
                if (!value)
                {
                    LocalConfigLastModified = -1;
                }
            }
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener(Action<string> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var wrap = new ManagerListenerWrap(listener, MD5);
            if (_listeners.AddIfNotExist(wrap))
            {
                _logger.Info($"[{_name}] [add-listener] ok, tenant={Tenant}, dataId={DataId}, group={Group}, cnt={_listeners.Count}");
            }
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(Action<string> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            var wrap = new ManagerListenerWrap(listener, MD5);
            if (_listeners.Remove(wrap))
            {
                _logger.Info($"[{_name}] [remove-listener] ok, dataId={DataId}, group={Group}, cnt={_listeners.Count}");
            }
        }

        /// <summary>
        /// 获取监听列表
        /// </summary>
        public IReadOnlyList<Action<string>> GetListeners()
        {
            var result = new List<Action<string>>();
            foreach(var wrap in _listeners.ToArray())
            {
                result.Add(wrap.Listener);
            }

            return result;
        }

        public static string GetMD5String(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            return CryptoUtils.GetMD5String(content);
        }

        public void CheckListenerMD5()
        {
            foreach(var wrap in _listeners.ToArray())
            {
                if (!MD5.Equals(wrap.LastCallMD5))
                {
                    SafeNotifyListener(DataId, Group, Content, MD5, wrap);
                }
            }
        }

        private void SafeNotifyListener(string dataId, string group, string content, string md5, ManagerListenerWrap listenerWrap)
        {
            // 此处为异步，当前为同步实现

            ConfigResponse cr = new ConfigResponse();
            cr.DataId = DataId;
            cr.Group = group;
            cr.Content = content;
            _configFilterChainManager.DoFilter(null, cr);
            listenerWrap.Listener(cr.Content);
            listenerWrap.LastCallMD5 = md5;

            _logger.Info($"[{_name}] [notify-ok] dataId={dataId}, group={group}, md5={md5}");
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + (string.IsNullOrEmpty(DataId) ? 0 : DataId.GetHashCode());
            result = prime * result + (string.IsNullOrEmpty(Group) ? 0 : Group.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            var othre = obj as CacheData;
            return DataId.Equals(othre.DataId) && Group.Equals(othre.Group);
        }

        public override string ToString()
        {
            return $"CacheData [{DataId}, {Group}]";
        }
    }
}
