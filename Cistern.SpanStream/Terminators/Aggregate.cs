using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct AggregationState { }

public struct Aggregate<T, TAccumulate>
    : IProcessStream<AggregationState, T, T, TAccumulate>
{
    private readonly Func<TAccumulate, T, TAccumulate> _func;

    public Aggregate(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) =>
        (_func, _accumulate) = (func, seed);

    private TAccumulate _accumulate;

    TAccumulate IProcessStream<AggregationState, T, T, TAccumulate>.GetResult(ref StreamState<T, AggregationState> state) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<AggregationState, T, T>.ProcessNext(ref StreamState<T, AggregationState> state, in T input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
