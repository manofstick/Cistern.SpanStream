using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;
using HonkPerf.NET.RefLinq;
using System.Collections.Immutable;

namespace Benchmarks;

/*

where-select-toarray
|     Method |    N |        Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Allocated |
|----------- |----- |------------:|----------:|----------:|-------:|-------:|----------:|
|     Manual |    0 |    30.46 ns |  0.023 ns |  0.019 ns | 0.0210 |      - |      88 B |
| SpanStream |    0 |    19.56 ns |  0.014 ns |  0.012 ns |      - |      - |         - |
|       Linq |    0 |    34.44 ns |  0.170 ns |  0.159 ns |      - |      - |         - |
|     Manual |    1 |    29.62 ns |  0.195 ns |  0.182 ns | 0.0210 |      - |      88 B |
| SpanStream |    1 |    74.27 ns |  0.274 ns |  0.256 ns |      - |      - |         - |
|       Linq |    1 |    66.98 ns |  0.245 ns |  0.191 ns | 0.0248 |      - |     104 B |
|     Manual |   10 |    57.95 ns |  0.283 ns |  0.264 ns | 0.0324 |      - |     136 B |
| SpanStream |   10 |   164.41 ns |  1.520 ns |  1.422 ns | 0.0114 |      - |      48 B |
|       Linq |   10 |   188.30 ns |  1.456 ns |  1.362 ns | 0.0591 |      - |     248 B |
|     Manual |  100 |   466.75 ns |  2.650 ns |  2.479 ns | 0.2041 |      - |     856 B |
| SpanStream |  100 |   975.21 ns |  4.364 ns |  3.868 ns | 0.0591 |      - |     248 B |
|       Linq |  100 | 1,181.97 ns |  5.353 ns |  4.745 ns | 0.1907 |      - |     800 B |
|     Manual | 1000 | 5,941.74 ns | 36.168 ns | 33.831 ns | 1.5030 |      - |   6,304 B |
| SpanStream | 1000 | 9,008.66 ns | 41.959 ns | 35.038 ns | 1.1902 | 0.0153 |   4,992 B |
|       Linq | 1000 | 9,224.28 ns | 15.755 ns | 13.966 ns | 1.0834 | 0.0153 |   4,544 B |

*/


/*
where
|     Method |     N |           Mean |       Error |      StdDev |  Gen 0 | Allocated |
|----------- |------ |---------------:|------------:|------------:|-------:|----------:|
|     Manual |     0 |      0.7675 ns |   0.0112 ns |   0.0171 ns |      - |         - |
| SpanStream |     0 |     19.0559 ns |   0.0115 ns |   0.0090 ns |      - |         - |
|       Linq |     0 |     11.7656 ns |   0.0057 ns |   0.0047 ns |      - |         - |
|     Manual |     1 |      5.5511 ns |   0.0030 ns |   0.0025 ns |      - |         - |
| SpanStream |     1 |     20.0931 ns |   0.2221 ns |   0.1968 ns |      - |         - |
|       Linq |     1 |     31.2678 ns |   0.7055 ns |   0.6929 ns | 0.0114 |      48 B |
|     Manual |    10 |     11.2439 ns |   0.2298 ns |   0.1919 ns |      - |         - |
| SpanStream |    10 |     77.2007 ns |   0.7145 ns |   0.6334 ns |      - |         - |
|       Linq |    10 |    115.1943 ns |   0.7445 ns |   0.6964 ns | 0.0114 |      48 B |
|     Manual |   100 |    336.3179 ns |   4.2141 ns |   3.5190 ns |      - |         - |
| SpanStream |   100 |    798.5584 ns |   2.3239 ns |   2.0601 ns |      - |         - |
|       Linq |   100 |    992.1810 ns |   4.1095 ns |   3.4316 ns | 0.0114 |      48 B |
|     Manual | 10000 | 49,176.6397 ns |   9.9341 ns |   8.8063 ns |      - |         - |
| SpanStream | 10000 | 76,101.9348 ns |  15.0898 ns |  11.7811 ns |      - |         - |
|       Linq | 10000 | 92,910.5981 ns | 290.1347 ns | 271.3921 ns |      - |      48 B |

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

select-where
|     Method |     N |           Mean |      Error |     StdDev |  Gen 0 | Allocated |
|----------- |------ |---------------:|-----------:|-----------:|-------:|----------:|
|     Manual |     0 |       4.960 ns |  0.0272 ns |  0.0254 ns |      - |         - |
| SpanStream |     0 |      29.400 ns |  0.0296 ns |  0.0231 ns |      - |         - |
|       Linq |     0 |      68.947 ns |  0.1348 ns |  0.1125 ns | 0.0134 |      56 B |
|     Manual |     1 |       5.346 ns |  0.0375 ns |  0.0351 ns |      - |         - |
| SpanStream |     1 |      32.339 ns |  0.1351 ns |  0.1197 ns |      - |         - |
|       Linq |     1 |      80.247 ns |  0.2980 ns |  0.2489 ns | 0.0248 |     104 B |
|     Manual |    10 |      51.821 ns |  0.1669 ns |  0.1479 ns |      - |         - |
| SpanStream |    10 |     126.330 ns |  1.1163 ns |  1.0442 ns |      - |         - |
|       Linq |    10 |     208.433 ns |  1.0826 ns |  0.9597 ns | 0.0248 |     104 B |
|     Manual |   100 |     327.587 ns |  1.0508 ns |  0.9829 ns |      - |         - |
| SpanStream |   100 |     875.638 ns |  2.8894 ns |  2.5614 ns |      - |         - |
|       Linq |   100 |   1,277.314 ns |  5.9211 ns |  5.5386 ns | 0.0248 |     104 B |
|     Manual | 10000 |  34,516.571 ns | 76.6347 ns | 63.9935 ns |      - |         - |
| SpanStream | 10000 |  87,071.903 ns | 14.7107 ns | 13.0407 ns |      - |         - |
|       Linq | 10000 | 123,752.885 ns | 58.5278 ns | 45.6946 ns |      - |     104 B |

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
    [Params(1, 10, 100, 1000)]
    public int N { get; set; }

    private ReadOnlyMemory<byte> data;
    private byte[] _asArray = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _asArray = new byte[N];
        new Random(42).NextBytes(_asArray);
        data = _asArray;

        _asArray[0] = 200;

        var tests = new Func<int>[]
        {
//            Manual,
            SpanStream,
            HonkPerf,
            Linq
        };

        var baseline = tests[0]();
        for (var i=1; i < tests.Length; ++i)
        {
            var check = tests[i]();
            Validate(baseline, check);
        }
    }
    private static void Validate<T>(T[] baseline, T[] check)
    {
        if (!baseline.SequenceEqual(check))
            throw new Exception("Validation error");
    }

    private static void Validate(int baseline, int check)
    {
        if (baseline!=check)
            throw new Exception("Validation error");
    }

    //[Benchmark]
    public int Manual()
    {
        //var x = ImmutableArray.CreateBuilder<int>();
        //var x = new int[data.Length];
        //var idx = 0;
        var accumulate = -59;
        foreach(var item in data.Span)
        {
            byte i = item ;
            //if (i < 250)
            {
                if (i > 128)
                {
                    accumulate += i * 2;
                    //x[idx++] = (i * 2);
//                    x.Add(i * 2);
                }
            }
        }
        return -accumulate;
        //return x.ToArray();
    }

    static readonly Func<byte, bool> AlwaysTrue = (byte b) => true;
    static readonly Func<byte, bool> AlwaysFalse = (byte b) => false;
    static readonly Func<byte, bool> FiftyFifty = (byte b) => b < 128;


    static readonly Func<byte, bool> CurrentPredicate = FiftyFifty;

    [Benchmark(Baseline =true)]
    public int SpanStream()
    {
        var x =
            data.Span
            .Where(CurrentPredicate);

        var sum = 0;
        foreach (var item in x)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int Linq()
    {
        var x =
            _asArray
            .Where(CurrentPredicate);

        var sum = 0;
        foreach (var item in x)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int HonkPerf()
    {
        var x =
            _asArray
            .ToRefLinq()
            .Where(CurrentPredicate);

        var sum = 0;
        foreach (var item in x)
            sum += item;
        return sum;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        //Array.Empty<int>().Aggregate((a, c) => a + c);

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

        //Cistern.Utils.StackAllocator.Allocate<int>(100);

        var zz = new FirstTest();
        zz.N = 100;
        zz.GlobalSetup();

        //return;

        var summary = BenchmarkRunner.Run<FirstTest>();
    }
}