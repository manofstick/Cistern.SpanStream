using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct ToArray<T>
    : IProcessStream<T, T, T[]>
{
    public ToArray() { }

    T[] IProcessStream<T, T, T[]>.GetResult(ref StreamState<T> state) => state.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        state.Add(input);
        return true;
    }
}

public struct ToArrayKnownSize<T>
    : IProcessStream<T, T, T[]>
{
    private T[] _array;
    private int _index;

    public ToArrayKnownSize(T[] arrayToFill) => (_array, _index) = (arrayToFill, 0);

    T[] IProcessStream<T, T, T[]>.GetResult(ref StreamState<T> state)
    {
        if (_array.Length != _index)
            ToArrayKnownSize<T>.DidntFillArray();
        return _array;
    }

    private static void DidntFillArray() => throw new Exception("_array.Length != _index");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _array[_index++] = input;
        return true;
    }
}
