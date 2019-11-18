using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NacosBenchmark
{
    /// <summary>
    /// 配置获取性能测试
    /// </summary>
    public class GetConfigBenchmark : ConfigBenchmarkBase
    {
        public GetConfigBenchmark()
            :base() { }

        [Benchmark]
        [Arguments("keyA", "tms")]
        [Arguments("keyB", "ccp")]
        public async Task GetExistedConfig(string dataId, string group)
        {
            string content = await ConfigService.GetConfig(dataId, group);
        }

        [Benchmark]
        [Arguments("NotFoundA", "tms")]
        [Arguments("NotFoundB", "ccp")]
        public async Task GetNotFoundConfig(string dataId, string group)
        {
            string content = await ConfigService.GetConfig(dataId, group);
        }
    }
}
