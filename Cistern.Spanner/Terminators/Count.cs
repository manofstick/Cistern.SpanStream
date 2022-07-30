using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Terminators;

public struct Count<T>
    : IProcessStream<T, T, int>
{
    public Count() => _count = 0;

    private int _count;

    int IProcessStream<T, T, int>.GetResult(ref StreamState<T> state) => _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        checked
        {
            ++_count;
            return true;
        }
    }
}
