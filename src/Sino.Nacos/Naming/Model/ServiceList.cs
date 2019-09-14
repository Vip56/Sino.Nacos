using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Model
{
    public class ServiceList
    {
        public long Count { get; set; }

        public IList<string> Doms { get; set; }
    }
}
