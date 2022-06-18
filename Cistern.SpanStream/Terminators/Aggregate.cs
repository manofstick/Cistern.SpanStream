using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct Aggregate<TSource, TAccumulate>
    : IProcessStream<TSource, TSource, TAccumulate>
{
    private readonly Func<TAccumulate, TSource, TAccumulate> _func;

    public Aggregate(Func<TAccumulate, TSource, TAccumulate> func, TAccumulate seed) =>
        (_func, _accumulate) = (func, seed);

    private TAccumulate _accumulate;

    TAccumulate IProcessStream<TSource, TSource, TAccumulate>.GetResult(ref Builder<TSource> builder) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TSource, TSource>.ProcessNext(ref Builder<TSource> builder, in TSource input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
