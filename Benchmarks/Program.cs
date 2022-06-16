using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;

namespace Benchmarks;

/*
|     Method |     N |          Mean |       Error |      StdDev |  Gen 0 | Allocated |
|----------- |------ |--------------:|------------:|------------:|-------:|----------:|
|     Manual |     0 |      4.807 ns |   0.0458 ns |   0.0428 ns |      - |         - |
| SpanStream |     0 |     33.376 ns |   0.1291 ns |   0.1208 ns |      - |         - |
|       Linq |     0 |     11.163 ns |   0.0801 ns |   0.0750 ns |      - |         - |
|     Manual |     1 |      4.578 ns |   0.0388 ns |   0.0363 ns |      - |         - |
| SpanStream |     1 |     38.149 ns |   0.0162 ns |   0.0126 ns |      - |         - |
|       Linq |     1 |     30.418 ns |   0.3115 ns |   0.2913 ns | 0.0114 |      48 B |
|     Manual |    10 |     10.324 ns |   0.0133 ns |   0.0104 ns |      - |         - |
| SpanStream |    10 |     90.378 ns |   0.4418 ns |   0.4132 ns |      - |         - |
|       Linq |    10 |    114.849 ns |   0.6244 ns |   0.5535 ns | 0.0114 |      48 B |
|     Manual |   100 |    341.800 ns |   1.8958 ns |   1.7734 ns |      - |         - |
| SpanStream |   100 |    848.996 ns |   3.2423 ns |   2.8742 ns |      - |         - |
|       Linq |   100 |    994.690 ns |   4.4373 ns |   4.1507 ns | 0.0114 |      48 B |
|     Manual | 10000 | 48,754.260 ns | 258.7518 ns | 242.0366 ns |      - |         - |
| SpanStream | 10000 | 81,215.123 ns | 319.0773 ns | 298.4651 ns |      - |         - |
|       Linq | 10000 | 93,293.412 ns | 484.0201 ns | 452.7527 ns |      - |      48 B |

*/
[Config(typeof(MyEnvVars))]
[MemoryDiagnoser]
public class FirstTest
{
    class MyEnvVars : ManualConfig
    {
        public MyEnvVars()
        {
            // Use .NET 6.0 default mode:
            //AddJob(Job.Default.WithId("Default mode"));

            // Use Dynamic PGO mode:
            AddJob(Job.Default.WithId("Dynamic PGO")
                .WithEnvironmentVariables(
                    new EnvironmentVariable("DOTNET_TieredPGO", "1"),
                    new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "1"),
                    new EnvironmentVariable("DOTNET_ReadyToRun", "0")));
        }
    }

    //[Params(0)]
    [Params(0, 1, 10, 100, 10000)]
    public int N { get; set; }

    private ReadOnlyMemory<byte> data;
    private byte[] _asArray = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _asArray = new byte[N];
        new Random(42).NextBytes(_asArray);
        data = _asArray;
         
        var a = Manual();
        var b = SpanStream();
        var c = Linq();
        if (a != b || a != c)
            throw new Exception("failed validation");
    }

    [Benchmark]
    public int Manual()
    {
        var accumulate = 0;
        foreach(var item in data.Span)
        {
            if (item > 128)
                accumulate += item;
        }
        return accumulate;
    }

    [Benchmark]
    public int SpanStream()
    {
        return
            data
            .ToSpanStream()
            .Where(x => x > 128)
            .Aggregate(0, (a,c) => a+c);
    }

    [Benchmark]
    public int Linq()
    {
        return
            _asArray
            .Where(x => x > 128)
            .Aggregate(0, (a, c) => a + c);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        //var ttotal = 0;
        //for (var j = 0; j < 20; ++j)
        //{
        //    var d = new[] { 129 };
        //    for (var i = 0; i < 10000000; ++i)
        //    {
        //        ttotal +=
        //            d.AsSpan()
        //            .ToSpanStream()
        //            .Where(x => x > 128)
        //            .Aggregate(0, (a, c) => a + c);
        //    }
        //    Console.Write('.');
        //}
        //Console.WriteLine(ttotal);
        //return;

        var zz = new FirstTest();
        zz.N = 10;
        zz.GlobalSetup();

        //return;

        var summary = BenchmarkRunner.Run<FirstTest>();
    }
}