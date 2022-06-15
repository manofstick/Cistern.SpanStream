using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public struct SumForward
    : IPushEnumerator<int>
{
    private int _accumulate;

    public SumForward() { _accumulate = 0; }

    TResult IPushEnumerator<int>.GetResult<TResult>() => (TResult)(object)GetResult()!;

    public int GetResult() => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IPushEnumerator<int>.ProcessNext(int input)
    {
        _accumulate += input;
        return true;
    }
}
