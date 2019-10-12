using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sino.Nacos.Config.Core
{
    /// <summary>
    /// 配置监听
    /// </summary>
    public interface IListener
    {
        Task GetTask();

        void ReceiveConfigInfo(string configInfo);
    }
}
