using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public struct FoldForward<T, TAccumulate>
    : IPushEnumerator<T>
{
    private TAccumulate _accumulate;
    private Func<TAccumulate, T, TAccumulate> _func;

    public FoldForward(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) => (_func, _accumulate) = (func, seed);

    TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult()!;

    public TAccumulate GetResult() => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IPushEnumerator<T>.ProcessNext(T input)
    {
        _accumulate = _func(_accumulate, input);
        return true;
    }
}
