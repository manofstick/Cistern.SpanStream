using Cistern.Utils;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct ToArrayState<T>
{
    readonly ArrayPool<T>? _maybePool;
    readonly int? _upperBound;

    int _bufferCount;
    int _nextIdx;
    int _count;

    public void Dispose(ref StreamState<T> container)
    {
        if (_maybePool == null)
            return;

        for (var idx = 0; idx < _bufferCount; ++idx)
            _maybePool.Return(container.Buffers[idx]!);
    }

    public void Add(ref StreamState<T> container, T item)
    {
        if (_nextIdx == container.Current.Length)
            AllocateNext(ref container);

        container.Current[_nextIdx] = item;

        ++_count;
        ++_nextIdx;
    }
    private void AllocateNext(ref StreamState<T> container)
    {
        var nextSize = container.Current.Length * 2;
        if (_count + nextSize > _upperBound)
        {
            nextSize = _upperBound.Value - _count;
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
    ToArrayState<T> _state;

    public ToArray() => _state = default;

    T[] IProcessStream<T, T, T[]>.GetResult(ref StreamState<T> state) => _state.ToArray(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _state.Add(ref state, input);
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
