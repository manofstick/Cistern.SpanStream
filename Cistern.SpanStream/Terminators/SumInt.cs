using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct SumForwardState { }

public struct SumForward
    : IProcessStream<SumForwardState, int, int, int>
{
    private int _accumulate;

    public SumForward() { _accumulate = 0; }

    int IProcessStream<SumForwardState, int, int, int>.GetResult(ref StreamState<int, SumForwardState> state) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<SumForwardState, int, int>.ProcessNext(ref StreamState<int, SumForwardState> state, in int input)
    {
        _accumulate += input;
        return true;
    }
}
