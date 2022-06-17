using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct SumForward
    : IProcessStream<int, int>
{
    private int _accumulate;

    public SumForward() { _accumulate = 0; }

    int IProcessStream<int, int>.GetResult() => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<int>.ProcessNext(in int input)
    {
        _accumulate += input;
        return true;
    }
}
