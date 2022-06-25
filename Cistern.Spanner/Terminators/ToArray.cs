using Cistern.Utils;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Terminators;

public struct ArrayBuilder<T>
{
    readonly int _upperBound;
    readonly ArrayPool<T>? _maybePool;

    int _bufferCount;
    int _nextIdx;
    int _count;

    public ArrayBuilder(int upperBound, ArrayPool<T>? maybePool)
    {
        _upperBound = upperBound;
        _maybePool = maybePool;

        _bufferCount = 0;
        _nextIdx = 0;
        _count = 0;
    }

    public void Dispose(ref StreamState<T> container)
    {
        if (_maybePool == null)
            return;

        for (var idx = 0; idx < _bufferCount; ++idx)
            _maybePool.Return(container.Buffers[idx]!);
    }

    public ref T GetNextCell(ref StreamState<T> container)
    {
        if (_nextIdx == container.Current.Length)
            AllocateNext(ref container);

        ++_count;
        ++_nextIdx;

        return ref container.Current[_nextIdx-1];
    }
    private void AllocateNext(ref StreamState<T> container)
    {
        var nextSize = container.Current.Length * 2;
        if (_count + nextSize > _upperBound)
        {
            nextSize = _upperBound - _count;
            if (nextSize <= 0)
                throw new IndexOutOfRangeException("Enumerator length has exceeded original count");
        }

        var newArray =
            _maybePool == null
                ? new T[nextSize]
                : _maybePool.Rent(nextSize);
        container.Buffers[_bufferCount++] = newArray;
        container.Current = newArray.AsSpan();
        _nextIdx = 0;
    }

    public T[] ToArray(ref StreamState<T> x)
    {
        if (_count == 0)
            return Array.Empty<T>();

        var array = new T[_count];

        var ptr = array.AsSpan();
        if (_bufferCount == 0)
        {
            x.Root[.._count].CopyTo(ptr);
        }
        else
        {
            x.Root.CopyTo(ptr);
            ptr = ptr[x.Root.Length..];
            for (var idx = 0; idx < _bufferCount - 1; ++idx)
            {
                var buffer = x.Buffers[idx].AsSpan();
                buffer.CopyTo(ptr);
                ptr = ptr[buffer.Length..];
            }
            x.Buffers[_bufferCount - 1].AsSpan(0, _nextIdx).CopyTo(ptr);
        }

        return array;
    }

    /*
    public ImmutableArray<T> ToImmutableArray()
    {
        if (_count == 0)
            return ImmutableArray<T>.Empty;

        var array = ImmutableArray.CreateBuilder<T>(_count);

        var head = _root[..Math.Min(_count, _root.Length)];
        foreach (var item in head)
            array.Add(item);
        for (var idx = 0; idx < _bufferCount - 1; ++idx)
            array.AddRange(_buffers[idx]!);
        if (_bufferCount > 0)
        {
            var tail =
                _buffers[_bufferCount - 1]
                .AsSpan(0, _nextIdx);
            foreach (var item in tail)
                array.Add(item);
        }

        return array.MoveToImmutable();
    }
    */
}

public struct ToArray<T>
    : IProcessStream<T, T, T[]>
{
    ArrayBuilder<T> _builder;

    public ToArray(int upperBound, ArrayPool<T>? maybePool)
    { 
        _builder = new (upperBound, maybePool);
    }

    T[] IProcessStream<T, T, T[]>.GetResult(ref StreamState<T> state)
    {
        var array = _builder.ToArray(ref state);
        _builder.Dispose(ref state);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _builder.GetNextCell(ref state) = input;
        return true;
    }

    public static T[] Execute<TInitial, TNode>(in ReadOnlySpan<TInitial> span, ref TNode source, int? stackElementCount, ArrayPool<T>? maybeArrayPool)
        where TNode : struct, IStreamNode<TInitial, T>
    {
        var maybeSize = source.TryGetSize(span.Length, out var upperBound);

        if (upperBound == 0)
            return Array.Empty<T>();

        if (maybeSize.HasValue)
        {
            ToArrayKnownSize<T> toArray = new(new T[maybeSize.Value]);
            return source.Execute<T, T[], ToArrayKnownSize<T>>(toArray, span, null);
        }
        else
        {
            ToArray<T> toArray = new(upperBound, maybeArrayPool);
            return source.Execute<T, T[], ToArray<T>>(toArray, span, Math.Min(upperBound, stackElementCount??0));
        }
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
