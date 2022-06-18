using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct ToArray<TSource>
    : IProcessStream<TSource, TSource, TSource[]>
{
    public ToArray() { }

    TSource[] IProcessStream<TSource, TSource, TSource[]>.GetResult(ref Builder<TSource> builder) => builder.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TSource, TSource>.ProcessNext(ref Builder<TSource> builder, in TSource input)
    {
        builder.Add(input);
        return true;
    }
}

public struct ToArrayKnownSize<TSource>
    : IProcessStream<TSource, TSource, TSource[]>
{
    private TSource[] _array;
    private int _index;

    public ToArrayKnownSize(TSource[] arrayToFill) => (_array, _index) = (arrayToFill, 0);

    TSource[] IProcessStream<TSource, TSource, TSource[]>.GetResult(ref Builder<TSource> builder)
    {
        if (_array.Length != _index)
            ToArrayKnownSize<TSource>.DidntFillArray();
        return _array;
    }

    private static void DidntFillArray() => throw new Exception("_array.Length != _index");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TSource, TSource>.ProcessNext(ref Builder<TSource> builder, in TSource input)
    {
        _array[_index++] = input;
        return true;
    }
}
