using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;

namespace Benchmarks;

/*
|     Method |     N |          Mean |       Error |      StdDev | Allocated |
|----------- |------ |--------------:|------------:|------------:|----------:|
|     Manual |     0 |      3.879 ns |   0.0332 ns |   0.0311 ns |         - |
| SpanStream |     0 |     44.955 ns |   0.2046 ns |   0.1709 ns |         - |
|     Manual |     1 |      4.964 ns |   0.0176 ns |   0.0165 ns |         - |
| SpanStream |     1 |     50.027 ns |   0.3521 ns |   0.3122 ns |         - |
|     Manual |    10 |     12.323 ns |   0.0791 ns |   0.0618 ns |         - |
| SpanStream |    10 |    103.127 ns |   0.5724 ns |   0.5354 ns |         - |
|     Manual |   100 |    329.219 ns |   0.9725 ns |   0.9097 ns |         - |
| SpanStream |   100 |    867.106 ns |   2.3711 ns |   2.2180 ns |         - |
|     Manual | 10000 | 49,405.724 ns | 252.8588 ns | 236.5243 ns |         - |
| SpanStream | 10000 | 81,648.793 ns | 414.4425 ns | 387.6698 ns |         - |

 
|     Method |     N |          Mean |       Error |      StdDev | Allocated |
|----------- |------ |--------------:|------------:|------------:|----------:|
|     Manual |     0 |      3.879 ns |   0.0210 ns |   0.0196 ns |         - |
| SpanStream |     0 |     44.440 ns |   0.0224 ns |   0.0175 ns |         - |
|     Manual |     1 |      4.961 ns |   0.0096 ns |   0.0085 ns |         - |
| SpanStream |     1 |     48.156 ns |   0.0209 ns |   0.0175 ns |         - |
|     Manual |    10 |     12.566 ns |   0.2769 ns |   0.2590 ns |         - |
| SpanStream |    10 |    100.570 ns |   0.5382 ns |   0.4771 ns |         - |
|     Manual |   100 |    333.951 ns |   0.5321 ns |   0.4717 ns |         - |
| SpanStream |   100 |    866.962 ns |   3.0384 ns |   2.8421 ns |         - |
|     Manual | 10000 | 49,216.669 ns |  15.5481 ns |  13.7830 ns |         - |
| SpanStream | 10000 | 81,625.052 ns | 399.7972 ns | 373.9706 ns |         - |

|     Method |     N |          Mean |         Error |        StdDev | Allocated |
|----------- |------ |--------------:|--------------:|--------------:|----------:|
|     Manual |     0 |      3.864 ns |     0.0191 ns |     0.0179 ns |         - |
| SpanStream |     0 |     43.129 ns |     0.1060 ns |     0.0939 ns |         - |
|     Manual |     1 |      4.957 ns |     0.0269 ns |     0.0239 ns |         - |
| SpanStream |     1 |     44.849 ns |     0.1486 ns |     0.1318 ns |         - |
|     Manual |    10 |     12.394 ns |     0.0636 ns |     0.0497 ns |         - |
| SpanStream |    10 |     98.277 ns |     0.6204 ns |     0.5500 ns |         - |
|     Manual |   100 |    333.667 ns |     0.7346 ns |     0.6872 ns |         - |
| SpanStream |   100 |    855.464 ns |     2.2747 ns |     2.1277 ns |         - |
|     Manual | 10000 | 49,199.876 ns |    12.3642 ns |    10.3247 ns |         - |
| SpanStream | 10000 | 83,462.156 ns | 1,491.1330 ns | 1,394.8067 ns |         - |


|     Method | N |      Mean |     Error |    StdDev | Allocated |
|----------- |-- |----------:|----------:|----------:|----------:|
|     Manual | 0 |  5.041 ns | 0.0373 ns | 0.0349 ns |         - |
| SpanStream | 0 | 34.859 ns | 0.1655 ns | 0.1548 ns |         - |
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

    [Params(0)]//, 1, 10, 100, 10000)]
    public int N { get; set; }

    private ReadOnlyMemory<byte> data;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var buffer = new byte[N];
        new Random(42).NextBytes(buffer);
        data = buffer;

         
        var a = Manual();
        var b = SpanStream();
        if (a != b)
            throw new Exception();
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

        var summary = BenchmarkRunner.Run<FirstTest>();
    }
}