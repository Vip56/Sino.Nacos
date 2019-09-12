using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Utilities
{
    public static class TimeUtil
    {
        public static long GetTimeStamp(this DateTime dt)
        {
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
