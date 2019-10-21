using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Core
{
    public class ManagerListenerWrap
    {
        public string LastCallMD5 { get; set; }

        public Action<string> Listener { get; private set; }

        public ManagerListenerWrap(Action<string> listener)
        {
            Listener = listener;
            LastCallMD5 = null;
        }

        public ManagerListenerWrap(Action<string> listener, string md5)
        {
            Listener = listener;
            LastCallMD5 = md5;
        }

        public override bool Equals(object obj)
        {
            if (null == obj || obj.GetType() == this.GetType())
                return false;
            if (obj == this)
                return true;

            var other = obj as ManagerListenerWrap;
            return Listener.Equals(other.LastCallMD5);
        }
    }
}
