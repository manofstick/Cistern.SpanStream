using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct Aggregate<T>
    : IProcessStream<T, T, T>
{
    private readonly Func<T, T, T> _func;

    public Aggregate(Func<T, T, T> func) => (_func, _first, _accumulate) = (func, true, default!);

    private bool _first;
    private T _accumulate;

    T IProcessStream<T, T, T>.GetResult(ref StreamState<T> state)
    {
        if (_first)
            ThrowHelper.SequenceContainsNoElements();
        return _accumulate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _accumulate = _first ? input : _func(_accumulate, input);
        _first = false;
        return true;
    }
}

public struct Aggregate<T, TAccumulate>
    : IProcessStream<T, T, TAccumulate>
{
    private readonly Func<TAccumulate, T, TAccumulate> _func;

    public Aggregate(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) =>
        (_func, _accumulate) = (func, seed);

    private TAccumulate _accumulate;

    TAccumulate IProcessStream<T, T, TAccumulate>.GetResult(ref StreamState<T> state) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}

public struct Aggregate<T, TAccumulate, TResult>
    : IProcessStream<T, T, TResult>
{
    private readonly Func<TAccumulate, T, TAccumulate> _func;
    private readonly Func<TAccumulate, TResult> _resultSelector;

    public Aggregate(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed, Func<TAccumulate, TResult> resultSelector) =>
        (_func, _accumulate, _resultSelector) = (func, seed, resultSelector);

    private TAccumulate _accumulate;

    TResult IProcessStream<T, T, TResult>.GetResult(ref StreamState<T> state) => _resultSelector(_accumulate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
