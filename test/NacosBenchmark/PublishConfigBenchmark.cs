using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NacosBenchmark
{
    public class PublishConfigBenchmark : ConfigBenchmarkBase
    {
        public PublishConfigBenchmark()
            : base() { }

        [Benchmark]
        [Arguments("ChangeA", "tms", "valueA")]
        [Arguments("ChangeB", "ccp", "valueB")]
        public async Task PublishConfig(string dataId, string group, string value)
        {
            bool result = await ConfigService.PublishConfig(dataId, group, value);
        }
    }
}
