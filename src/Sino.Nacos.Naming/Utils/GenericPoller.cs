using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sino.Nacos.Naming.Utils
{
    public class GenericPoller<T> : IPoller<T>
    {
        private int _index = 0;
        private IList<T> _items;

        public GenericPoller(IList<T> items)
        {
            _items = items;
        }

        public T Next()
        {
            return _items[Math.Abs(Interlocked.Increment(ref _index) % _items.Count)];
        }

        public IPoller<T> Refresh(IList<T> items)
        {
            return new GenericPoller<T>(items);
        }
    }
}
