using Sino.Nacos.Naming.Net;
using System.Collections.Concurrent;
using Sino.Nacos.Naming.Model;
using NLog;
using System.Threading;
using System;

namespace Sino.Nacos.Naming.Beat
{
    /// <summary>
    /// 用于提供注册服务后的心跳
    /// </summary>
    /// <remarks>
    /// 考虑到实际场景中一个服务可以注册成为多个不同服务名称或
    /// 将不同出口IP地址和端口进行注册，而每个独立服务需要单独
    /// 的心跳以保持Nacos不会将自己注册的服务置为不健康。
    /// </remarks>
    public class BeatReactor
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private NamingProxy _serverProxy;
        private ConcurrentDictionary<string, BeatInfo> _dom2beat = new ConcurrentDictionary<string, BeatInfo>();
        private ConcurrentDictionary<string, Timer> _beatTimer = new ConcurrentDictionary<string, Timer>();

        public BeatReactor(NamingProxy serverProxy)
        {
            _serverProxy = serverProxy;
        }

        /// <summary>
        /// 添加心跳
        /// </summary>
        public void AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger.Info($"[BEAT] adding beat: {beatInfo} to beat map.");
            string key = BuildKey(serviceName, beatInfo.Ip, beatInfo.Port);
            _dom2beat.AddOrUpdate(key, beatInfo, (x, s) => beatInfo);
            Timer t = new Timer(async x =>
            {
                var state = x as Tuple<BeatInfo, string>;
                Timer child = null;
                if (state.Item1.Stopped)
                {
                    return;
                }
                long result = await _serverProxy.SendBeat(state.Item1);
                long nextTime = result > 0 ? result : state.Item1.PerId;
                
                if (_beatTimer.TryGetValue(BuildKey(state.Item2, state.Item1.Ip, state.Item1.Port), out child))
                {
                    child.Change(nextTime, Timeout.Infinite);
                }
            }, Tuple.Create(beatInfo, serviceName), Timeout.Infinite, Timeout.Infinite);
            _beatTimer.AddOrUpdate(key, t, (x, s) => t);

            t.Change(0, Timeout.Infinite);
        }

        /// <summary>
        /// 移除心跳
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void RemoveBeatInfo(string serviceName, string ip, int port)
        {
            _logger.Info($"[BEAT] removing beat: {serviceName} {ip} {port} to beat map.");
            BeatInfo info = null;
            Timer t = null;
            string key = BuildKey(serviceName, ip, port);
            if (_dom2beat.TryRemove(key, out info))
            {
                info.Stopped = true;
            }
            if (_beatTimer.TryRemove(key, out t))
            {
                t.Dispose();
            }
        }

        private string BuildKey(string serviceName, string ip, int port)
        {
            return serviceName + Constants.NAMING_INSTANCE_ID_SPLITTER + ip + Constants.NAMING_INSTANCE_ID_SPLITTER + port.ToString();
        }
    }
}
