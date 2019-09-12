using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sino.Nacos.Config.Listener
{
    /// <summary>
    /// 监听配置
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// 触发监听接收器
        /// </summary>
        Task GetTask();

        /// <summary>
        /// 获取配置信息
        /// </summary>
        void ReceiveConfigInfo(string configInfo);
    }
}
