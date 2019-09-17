using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Utils
{
    public interface IPoller<T>
    {
        T Next();

        IPoller<T> Refresh(IList<T> items);
    }
}
