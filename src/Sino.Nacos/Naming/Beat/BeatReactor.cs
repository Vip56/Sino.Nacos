using Sino.Nacos.Naming.Net;
using System.Collections.Concurrent;
using Sino.Nacos.Naming.Model;
using Sino.Nacos.Common;
using NLog;
using System.Threading;

namespace Sino.Nacos.Naming.Beat
{
    /// <summary>
    /// 服务心跳
    /// </summary>
    public class BeatReactor
    {
        private NamingProxy _serverProxy;
        private ConcurrentDictionary<string, BeatInfo> _dom2beat = new ConcurrentDictionary<string, BeatInfo>();
        private ConcurrentDictionary<string, Timer> _beatTimer = new ConcurrentDictionary<string, Timer>();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BeatReactor(NamingProxy serverProxy)
        {
            _serverProxy = serverProxy;
        }

        public void AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger.Info($"[BEAT] adding beat: {beatInfo} to beat map.");
            string key = BuildKey(serviceName, beatInfo.Ip, beatInfo.Port);
            _dom2beat.AddOrUpdate(key, beatInfo, (x, s) => beatInfo);
            Timer t = new Timer(async x =>
            {
                var state = x as TimeState;
                Timer child = null;
                if (state.BeatInfo.Stopped)
                {
                    return;
                }
                long result = await _serverProxy.SendBeat(state.BeatInfo);
                long nextTime = result > 0 ? result : state.BeatInfo.PerId;
                
                if (_beatTimer.TryGetValue(BuildKey(state.ServiceName, state.BeatInfo.Ip, state.BeatInfo.Port), out child))
                {
                    child.Change(nextTime, Timeout.Infinite);
                }
            }, new TimeState(beatInfo, serviceName), Timeout.Infinite, Timeout.Infinite);
            _beatTimer.AddOrUpdate(key, t, (x, s) => t);

            t.Change(0, Timeout.Infinite);
        }

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

        public class TimeState
        {
            public BeatInfo BeatInfo { get; set; }

            public string ServiceName { get; set; }

            public TimeState(BeatInfo beatInfo, string serviceName)
            {
                this.BeatInfo = beatInfo;
                this.ServiceName = serviceName;
            }
        }
    }
}
