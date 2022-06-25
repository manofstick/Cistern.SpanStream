using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Cistern.SpanStream;
using HonkPerf.NET.RefLinq;
using System.Collections.Immutable;

namespace Benchmarks;

/*
    data.Span
    .Append(42)
    .Where(FiftyFifty)
    .Append(42)
    .Select(x => x * 2)
    .Append(42)
    .Where(AlwaysTrue)

_Sum ==
    .Aggregate(0, (a, c) => a+c)
Else ==
    var sum = 0;
    foreach (var item in pipeline)
        sum += item;
    return sum;

|         Method |    N |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |  Gen 0 | Allocated |
|--------------- |----- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|
|     SpanStream |    1 |    218.9 ns |   1.38 ns |   1.29 ns |    218.6 ns |  0.88 |    0.00 |      - |         - |
| SpanStream_Sum |    1 |    247.6 ns |   0.79 ns |   0.74 ns |    247.1 ns |  1.00 |    0.00 |      - |         - |
|           Linq |    1 |    368.9 ns |   2.36 ns |   2.09 ns |    369.4 ns |  1.49 |    0.01 | 0.0877 |     368 B |
|       Linq_Sum |    1 |    370.9 ns |   1.57 ns |   1.47 ns |    370.6 ns |  1.50 |    0.01 | 0.0877 |     368 B |
|       HonkPerf |    1 |    108.6 ns |   0.20 ns |   0.39 ns |    108.4 ns |  0.44 |    0.00 |      - |         - |
|   HonkPerf_Sum |    1 |    177.8 ns |   0.61 ns |   0.54 ns |    177.8 ns |  0.72 |    0.00 |      - |         - |
|                |      |             |           |           |             |       |         |        |           |
|     SpanStream |   10 |    388.8 ns |   2.03 ns |   1.90 ns |    388.2 ns |  1.07 |    0.01 |      - |         - |
| SpanStream_Sum |   10 |    362.7 ns |   4.74 ns |   4.43 ns |    360.0 ns |  1.00 |    0.00 |      - |         - |
|           Linq |   10 |    679.4 ns |   3.10 ns |   2.90 ns |    677.7 ns |  1.87 |    0.03 | 0.0877 |     368 B |
|       Linq_Sum |   10 |    643.9 ns |   3.49 ns |   2.92 ns |    644.2 ns |  1.78 |    0.02 | 0.0877 |     368 B |
|       HonkPerf |   10 |    261.2 ns |   1.46 ns |   1.37 ns |    261.6 ns |  0.72 |    0.01 |      - |         - |
|   HonkPerf_Sum |   10 |    344.1 ns |   1.92 ns |   1.70 ns |    343.5 ns |  0.95 |    0.01 |      - |         - |
|                |      |             |           |           |             |       |         |        |           |
|     SpanStream |  100 |  1,851.3 ns |   8.78 ns |   8.22 ns |  1,851.5 ns |  1.24 |    0.01 |      - |         - |
| SpanStream_Sum |  100 |  1,495.2 ns |   5.16 ns |   4.57 ns |  1,492.1 ns |  1.00 |    0.00 |      - |         - |
|           Linq |  100 |  3,116.7 ns |  19.14 ns |  17.90 ns |  3,117.8 ns |  2.09 |    0.01 | 0.0877 |     368 B |
|       Linq_Sum |  100 |  3,156.1 ns |  16.66 ns |  15.58 ns |  3,154.0 ns |  2.11 |    0.01 | 0.0877 |     368 B |
|       HonkPerf |  100 |  1,716.2 ns |   7.70 ns |   7.20 ns |  1,717.1 ns |  1.15 |    0.01 |      - |         - |
|   HonkPerf_Sum |  100 |  2,054.1 ns |  10.01 ns |   9.37 ns |  2,047.1 ns |  1.37 |    0.01 |      - |         - |
|                |      |             |           |           |             |       |         |        |           |
|     SpanStream | 1000 | 15,778.8 ns |  81.16 ns |  75.92 ns | 15,726.3 ns |  1.12 |    0.01 |      - |         - |
| SpanStream_Sum | 1000 | 14,075.2 ns |  69.54 ns |  65.05 ns | 14,028.5 ns |  1.00 |    0.00 |      - |         - |
|           Linq | 1000 | 30,947.8 ns | 156.06 ns | 145.98 ns | 30,863.5 ns |  2.20 |    0.01 | 0.0610 |     368 B |
|       Linq_Sum | 1000 | 32,218.5 ns |  12.25 ns |   9.56 ns | 32,218.6 ns |  2.29 |    0.01 | 0.0610 |     368 B |
|       HonkPerf | 1000 | 16,695.7 ns |   6.56 ns |   5.48 ns | 16,697.8 ns |  1.19 |    0.01 |      - |         - |
|   HonkPerf_Sum | 1000 | 20,622.5 ns |  93.68 ns |  87.63 ns | 20,664.0 ns |  1.47 |    0.01 |      - |         - |

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

#if LONG
    //[Params(1, 10, 100, 1000)]
#endif
    [Params(100)]
    public int N { get; set; }

    private ReadOnlyMemory<int> data;
    private int[] _asArray = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var r = new Random(42);
        _asArray =
            System.Linq.Enumerable
            .Range(0, N)
            .Select(n => r.Next(n * 5))
            .ToArray();
        data = _asArray;

        var tests = new Func<int>[]
        {
            Manual,
            //SpanStream,
            SpanStream_Enumerator,
            Linq,
            Linq_Enumerator,
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

    private static void Validate(int? baseline, int? check)
    {
        if (!Nullable.Equals(baseline, check))
            throw new Exception("Validation error");
    }

    static readonly Func<int, bool> AlwaysTrue = (int b) => true;
    static readonly Func<int, bool> AlwaysFalse = (int b) => false;
    static readonly Func<int, bool> FiftyFifty = (int b) => b % 2 == 0;

    static readonly Func<int, bool> CurrentPredicate = FiftyFifty;

    [Benchmark]
    public int Manual()
    {
        var sum = 0;
        for(var i= _asArray.Length-1; i >= 0; --i)
            sum = (sum * sum) + _asArray[i];
        return sum;
    }

    //[Benchmark]
    public int SpanStream()
    {
        return
            data.Span
            .Select(x => x)
            .Reverse()
            .Aggregate((a, c) => (a * a) + c);
    }

    [Benchmark]
    public int SpanStream_Enumerator()
    {
        var x =
            data.Span
            .Select(x => x)
            .Reverse();

        var sum = 0;
        foreach (var item in x)
            sum = (sum * sum) + item;
        return sum;
    }

    [Benchmark]
    public int Linq()
    {
        return
            _asArray
            .Select(x => x)
            .Reverse()
            .Aggregate((a, c) => (a * a) + c);
    }

    [Benchmark]
    public int Linq_Enumerator()
    {
        var x =
            _asArray
            .Select(x => x)
            .Reverse();

        var sum = 0;
        foreach (var item in x)
            sum = (sum * sum) + item;
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