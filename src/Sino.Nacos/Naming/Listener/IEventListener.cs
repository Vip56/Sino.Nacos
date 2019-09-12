using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Listener
{
    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEventListener
    {
        void OnEvent(IEvent @event);
    }
}
