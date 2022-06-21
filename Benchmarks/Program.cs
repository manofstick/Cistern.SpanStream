using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;
using System.Collections.Immutable;

namespace Benchmarks;

/*

where-select-toarray
|     Method |     N |         Mean |      Error |     StdDev |   Gen 0 |  Gen 1 | Allocated |
|----------- |------ |-------------:|-----------:|-----------:|--------:|-------:|----------:|
|     Manual |     0 |     25.30 ns |   0.199 ns |   0.186 ns |  0.0210 |      - |      88 B |
| SpanStream |     0 |     58.97 ns |   0.360 ns |   0.337 ns |       - |      - |         - |
|       Linq |     0 |     34.16 ns |   0.132 ns |   0.123 ns |       - |      - |         - |
|     Manual |     1 |     29.93 ns |   0.166 ns |   0.155 ns |  0.0210 |      - |      88 B |
| SpanStream |     1 |     85.29 ns |   0.071 ns |   0.059 ns |       - |      - |         - |
|       Linq |     1 |     82.01 ns |   0.464 ns |   0.434 ns |  0.0248 |      - |     104 B |
|     Manual |    10 |     58.55 ns |   0.297 ns |   0.278 ns |  0.0324 |      - |     136 B |
| SpanStream |    10 |    161.19 ns |   0.880 ns |   0.823 ns |  0.0114 |      - |      48 B |
|       Linq |    10 |    200.11 ns |   0.893 ns |   0.836 ns |  0.0591 |      - |     248 B |
|     Manual |   100 |    368.88 ns |   1.923 ns |   1.799 ns |  0.2046 |      - |     856 B |
| SpanStream |   100 |    982.76 ns |   4.660 ns |   3.638 ns |  0.0591 |      - |     248 B |
|       Linq |   100 |  1,176.57 ns |   6.249 ns |   5.846 ns |  0.1907 |      - |     800 B |
|     Manual | 10000 | 66,672.66 ns | 796.538 ns | 745.082 ns | 15.0146 | 7.4463 |  85,720 B |
| SpanStream | 10000 | 87,662.42 ns | 399.659 ns | 373.841 ns |  3.5400 | 1.7090 |  19,920 B |
|       Linq | 10000 | 91,712.54 ns | 437.863 ns | 388.155 ns |  9.0332 | 4.5166 |  53,392 B |

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
    [Params(0, 1, 10, 100, 1000)]
    public int N { get; set; }

    private ReadOnlyMemory<byte> data;
    private byte[] _asArray = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _asArray = new byte[N];
        new Random(42).NextBytes(_asArray);
        data = _asArray;

        var tests = new Func<int[]>[]
        {
            Manual,
            SpanStream,
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

    [Benchmark]
    public int[] Manual()
    {
        var x = ImmutableArray.CreateBuilder<int>();
        //var x = new int[data.Length];
        //var idx = 0;
        //var accumulate = 0;
        foreach(var item in data.Span)
        {
            byte i = item ;
            //if (i < 250)
            {
                if (i > 128)
                {
                    //accumulate += i;
                    //x[idx++] = (i * 2);
                    x.Add(i * 2);
                }
            }
        }
//        return accumulate;
        return x.ToArray();
    }

    [Benchmark]
    public int[] SpanStream()
    {
        return
            data.Span
//            .Where(x => x < 250)
            .Where(x => x > 128)
                                    .Select(x => x * 2)
                        .ToArray();
            //.Aggregate(0, (a, c) => a + c);
    }

    [Benchmark]
    public int[] Linq()
    {
        return
            _asArray
            //            .Where(x => x < 250)
            .Where(x => x > 128)
                                    .Select(x => x * 2)
                        .ToArray();
            //.Aggregate(0, (a, c) => a + c);
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

        //Cistern.Utils.StackAllocator.Allocate<int>(100);

        var zz = new FirstTest();
        zz.N = 1;
        zz.GlobalSetup();

        //return;

        var summary = BenchmarkRunner.Run<FirstTest>();
    }
}