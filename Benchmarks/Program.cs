using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;

namespace Benchmarks;

/*
where
|     Method |     N |          Mean |       Error |      StdDev |  Gen 0 | Allocated |
|----------- |------ |--------------:|------------:|------------:|-------:|----------:|
|     Manual |     0 |      4.739 ns |   0.0296 ns |   0.0277 ns |      - |         - |
| SpanStream |     0 |     22.213 ns |   0.1215 ns |   0.1137 ns |      - |         - |
|       Linq |     0 |     11.141 ns |   0.0580 ns |   0.0543 ns |      - |         - |
|     Manual |     1 |      4.532 ns |   0.0314 ns |   0.0278 ns |      - |         - |
| SpanStream |     1 |     24.364 ns |   0.1637 ns |   0.1532 ns |      - |         - |
|       Linq |     1 |     31.334 ns |   0.2093 ns |   0.1748 ns | 0.0114 |      48 B |
|     Manual |    10 |      7.420 ns |   0.1258 ns |   0.2237 ns |      - |         - |
| SpanStream |    10 |     82.343 ns |   0.5786 ns |   0.5129 ns |      - |         - |
|       Linq |    10 |    114.951 ns |   0.6082 ns |   0.5391 ns | 0.0114 |      48 B |
|     Manual |   100 |    335.089 ns |   1.0011 ns |   0.9364 ns |      - |         - |
| SpanStream |   100 |    800.837 ns |   2.6049 ns |   2.1752 ns |      - |         - |
|       Linq |   100 |  1,017.344 ns |  18.9634 ns |  16.8106 ns | 0.0114 |      48 B |
|     Manual | 10000 | 49,128.252 ns | 128.1920 ns | 119.9108 ns |      - |         - |
| SpanStream | 10000 | 76,019.536 ns | 246.5020 ns | 218.5176 ns |      - |         - |
|       Linq | 10000 | 92,976.025 ns | 393.6327 ns | 368.2043 ns |      - |      48 B |

where-select
|     Method |     N |           Mean |       Error |      StdDev |         Median |  Gen 0 | Allocated |
|----------- |------ |---------------:|------------:|------------:|---------------:|-------:|----------:|
|     Manual |     0 |       4.749 ns |   0.0466 ns |   0.0364 ns |       4.748 ns |      - |         - |
| SpanStream |     0 |      27.296 ns |   0.6508 ns |   1.2063 ns |      26.501 ns |      - |         - |
|       Linq |     0 |      31.738 ns |   0.2333 ns |   0.2182 ns |      31.597 ns |      - |         - |
|     Manual |     1 |       5.441 ns |   0.0488 ns |   0.0433 ns |       5.418 ns |      - |         - |
| SpanStream |     1 |      31.050 ns |   0.1393 ns |   0.1303 ns |      30.972 ns |      - |         - |
|       Linq |     1 |      68.950 ns |   0.5765 ns |   0.5392 ns |      68.810 ns | 0.0248 |     104 B |
|     Manual |    10 |      11.597 ns |   0.0043 ns |   0.0040 ns |      11.597 ns |      - |         - |
| SpanStream |    10 |      95.261 ns |   0.6645 ns |   0.5890 ns |      95.098 ns |      - |         - |
|       Linq |    10 |     160.951 ns |   0.7898 ns |   0.7388 ns |     160.873 ns | 0.0248 |     104 B |
|     Manual |   100 |     342.428 ns |   2.3003 ns |   2.0392 ns |     342.630 ns |      - |         - |
| SpanStream |   100 |     914.096 ns |   6.9692 ns |   6.5190 ns |     911.414 ns |      - |         - |
|       Linq |   100 |   1,115.213 ns |   3.9219 ns |   3.6686 ns |   1,113.182 ns | 0.0248 |     104 B |
|     Manual | 10000 |  49,452.871 ns | 164.9074 ns | 146.1861 ns |  49,419.406 ns |      - |         - |
| SpanStream | 10000 |  87,952.950 ns | 351.3591 ns | 311.4707 ns |  87,897.699 ns |      - |         - |
|       Linq | 10000 | 101,768.123 ns | 312.2823 ns | 292.1090 ns | 101,551.819 ns |      - |     104 B |

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
                accumulate += item * 2;
        }
        return accumulate;
    }

    [Benchmark]
    public int SpanStream()
    {
        return
            data.Span
            .Where(x => x > 128)
            .Select(x => x * 2)
            .Aggregate(0, (a, c) => a + c);
    }

    [Benchmark]
    public int Linq()
    {
        return
            _asArray
            .Where(x => x > 128)
            .Select(x => x * 2)
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