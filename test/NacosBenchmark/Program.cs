using BenchmarkDotNet.Running;
using System;

namespace NacosBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            // 测试获取配置
            // var summary = BenchmarkRunner.Run<GetConfigBenchmark>();

            // 测试发布配置
            var summary = BenchmarkRunner.Run<PublishConfigBenchmark>();

            Console.ReadLine();
        }
    }
}
