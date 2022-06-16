using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public struct Aggregate<TSource, TAccumulate>
    : IProcessStream<TSource, TAccumulate>
{
    private readonly Func<TAccumulate, TSource, TAccumulate> _func;

    public Aggregate(Func<TAccumulate, TSource, TAccumulate> func, TAccumulate seed) =>
        (_func, _accumulate) = (func, seed);

    private TAccumulate _accumulate;

    TAccumulate IProcessStream<TSource, TAccumulate>.GetResult() => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TSource, TAccumulate>.ProcessNext(TSource input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
