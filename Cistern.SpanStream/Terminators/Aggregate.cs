using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct Aggregate<T, TAccumulate>
    : IProcessStream<T, T, TAccumulate>
{
    private readonly Func<TAccumulate, T, TAccumulate> _func;

    public Aggregate(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) =>
        (_func, _accumulate) = (func, seed);

    private TAccumulate _accumulate;

    TAccumulate IProcessStream<T, T, TAccumulate>.GetResult(ref Builder<T> builder) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref Builder<T> builder, in T input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
