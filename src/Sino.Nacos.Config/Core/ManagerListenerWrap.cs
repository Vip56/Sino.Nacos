﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Core
{
    public class ManagerListenerWrap
    {
        private IListener _listener;
        private string _lastCallmd5;

        ManagerListenerWrap(IListener listener)
        {
            _listener = listener;
            _lastCallmd5 = null;
        }

        ManagerListenerWrap(IListener listener, string md5)
        {
            _listener = listener;
            _lastCallmd5 = md5;
        }

        public override bool Equals(object obj)
        {
            if (null == obj || obj.GetType() == this.GetType())
                return false;
            if (obj == this)
                return true;

            var other = obj as ManagerListenerWrap;
            return _listener.Equals(other._listener);
        }
    }
}