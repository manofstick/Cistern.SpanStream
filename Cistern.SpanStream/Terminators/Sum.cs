using Cistern.SpanStream.Maths;
using Cistern.Utils;

namespace Cistern.SpanStream.Terminators;

struct Sum<T, Accumulator, Quotient, Math>
    : IProcessStream<T, T, T>
    where T : struct
    where Accumulator : struct
    where Quotient : struct
    where Math : struct, IMathsOperations<T, Accumulator, Quotient>
{
    static Math math = default;

    private Accumulator _sum;

    T IProcessStream<T, T, T>.GetResult(ref StreamState<T> builder) => math.Cast(_sum);

    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> builder, in T input)
    {
        _sum = math.Add(_sum, input);
        return true;
    }

    public static T SimdSum(ReadOnlySpan<T> source, SIMDOptions options)
    {
        var result = math.Zero;
        SIMD.Sum<T, Accumulator, Quotient, Math>(source, options, ref result);
        return math.Cast(result);
    }
}

struct SumNullable<T, Accumulator, Quotient, Math>
    : IProcessStream<T?, T?, T?>
    where T : struct
    where Accumulator : struct
    where Quotient : struct
    where Math : struct, IMathsOperations<T, Accumulator, Quotient>
{
    static Math math = default;

    private Accumulator _sum;

    T? IProcessStream<T?, T?, T?>.GetResult(ref StreamState<T?> builder) => math.Cast(_sum);

    bool IProcessStream<T?, T?>.ProcessNext(ref StreamState<T?> builder, in T? input)
    {
        _sum = math.Add(_sum, input);
        return true;
    }
}
