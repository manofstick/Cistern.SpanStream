using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct SumForward
    : IProcessStream<int, int, int>
{
    private int _accumulate;

    public SumForward() { _accumulate = 0; }

    int IProcessStream<int, int, int>.GetResult(ref StreamState<int> state) => _accumulate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<int, int>.ProcessNext(ref StreamState<int> state, in int input)
    {
        _accumulate += input;
        return true;
    }
}
