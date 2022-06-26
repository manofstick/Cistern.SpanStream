using Cistern.Spanner.Roots;
using Cistern.Spanner.Terminators;
using Cistern.Spanner.Utils;
using Cistern.Utils;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Transforms;

struct KeySortable<T, TComparer, U>
    : IComparable<KeySortable<T, TComparer, U>>
    where U : IComparable<U>
    where TComparer : IComparer<T>
{
    TComparer _comparer;
    T _t;
    U _u;

    public KeySortable(T t, TComparer comparer, U u)
    {
        _t = t;
        _comparer = comparer;
        _u = u;
    }

    public int CompareTo(KeySortable<T, TComparer, U> other)
    {
        var c = _comparer.Compare(_t, other._t);
        if (c != 0)
            return c;
        return _u.CompareTo(other._u);
    }
}

public /*readonly*/ struct OrderBy<TInitial, TInput, TKey, TPriorNode>
    : IStreamNode<TInitial, TInput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    internal /*readonly*/ TPriorNode Node;
    Func<TInput, TKey> _getKey;
    IComparer<TKey> _keyComparer;

    private int _stackElementCount;
    private ArrayPool<TInput>? _maybeArrayPool;

    private int _index;
    private TInput[] _ordered;

    public OrderBy(ref TPriorNode nodeT, Func<TInput, TKey> getKey, IComparer<TKey>? keyComparer, int? stackElementCount, ArrayPool<TInput>? maybeArrayPool)
    {
        Node = nodeT;
        _getKey = getKey;
        _keyComparer = keyComparer ?? GetComparer();
        _stackElementCount = stackElementCount ?? 0;
        _maybeArrayPool = maybeArrayPool;
        _ordered = UninitializedArrayStandIn<TInput>.Instance;
        _index = int.MaxValue;
    }

    private static IComparer<TKey> GetComparer()
    {
        if (typeof(TKey) == typeof(string))
            return (IComparer<TKey>)StringComparer.CurrentCulture;
        return Comparer<TKey>.Default;
    }

    int? IStreamNode<TInitial, TInput>.TryGetSize(int sourceSize, out int upperBound) =>
        Node.TryGetSize(sourceSize, out upperBound);

    public TResult Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
        where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
    {
        var _ = Node.TryGetSize(span.Length, out var upperBound);
        if (upperBound <= _stackElementCount)
        {
            return Node.Execute<TInput, TResult, OrderByOnStreamState<TInput, TKey, TFinal, TResult, TProcessStream>>(new(in processStream, _getKey, _keyComparer), span, upperBound);
        }
        else
        {
            var reversedArray = CreateOrderByArray(span);
            return Root<TInput>.Instance.Execute<TFinal, TResult, TProcessStream>(processStream, reversedArray.ToReadOnlySpan(), 0);
        }
    }

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TInput current)
    {
        if (_index < _ordered.Length)
        {
            current = _ordered[_index++];
            return true;
        }
        return LesserPath(ref state, out current);
    }

    bool LesserPath(ref EnumeratorState<TInitial> state, out TInput current)
    {
        if (UninitializedArrayStandIn.IsArrayUninitialized(_ordered))
        {
            _index = 0;
            var span = state.Span[state.Index..];
            _ordered = CreateOrderByArray(in span);
            state.Index = state.Span.Length;
            return TryGetNext(ref state, out current);
        }
        current = default!;
        return false;
    }

    private TInput[] CreateOrderByArray(in ReadOnlySpan<TInitial> span)
    {
        var buffer = ToArray<TInput>.Execute(span, ref Node, _stackElementCount, _maybeArrayPool);
        if (buffer.Length <= _stackElementCount)
            return Root<TInput>.Instance.Execute<TKey, TInput[], OrderByArrayOnStreamState<TInput, TKey>>(new(buffer, _getKey, _keyComparer), buffer.AsSpan(), buffer.Length);
        else
        {
            var select = new SelectRoot<TInput, TKey>(_getKey);
            var keys = ToArray<TKey>.Execute(buffer.ToReadOnlySpan(), ref select, _stackElementCount, null);
            Array.Sort(keys, buffer, _keyComparer);
            return buffer;
        }
    }
}

public struct OrderByArrayOnStreamState<TInput, TKey>
    : IProcessStream<TInput, TKey, TInput[]>
{
    TInput[] _toSort;
    Func<TInput, TKey> _getKey;
    IComparer<TKey> _keyComparer;
    int _index;

    public OrderByArrayOnStreamState(TInput[] toSort, Func<TInput, TKey> getKey, IComparer<TKey> keyComparer) =>
        (_toSort, _getKey, _keyComparer, _index) = (toSort, getKey, keyComparer, 0);

    public TInput[] GetResult(ref StreamState<TKey> builder)
    {
        builder.Current[.._index].Sort(_toSort.AsSpan(), _keyComparer);
        return _toSort;
    }

    public bool ProcessNext(ref StreamState<TKey> builder, in TInput input)
    {
        builder.Current[_index++] = _getKey(input);
        return true;
    }
}

internal interface IAfterAllocation<TInitial, TNext, TState>
{
    TResult Execute<TFinal, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> builder, in TState state)
        where TProcessStream : struct, IProcessStream<TNext, TFinal, TResult>;
}


static class OrderByHelpers
{
    public static void SortByKey<T, TKey>(Span<T> source, Func<T, TKey> getKey, IComparer<TKey> comparer)
    {
        BuildStackToSort<T, TKey, LargeStackAllocator.BufferStorage<TKey>>(source, getKey, comparer, LargeStackAllocator.BufferStorage<TKey>.NumberOfElements);
    }

    private static void BuildStackToSort<T, TKey, TStackObj>(Span<T> source, Func<T, TKey> getKey, IComparer<TKey> comparer, int stackObjSize)
        where TStackObj : struct
    {
        if (stackObjSize >= source.Length)
            DoSort<T, TKey, TStackObj>(source, getKey, comparer);
        else
            BuildStackToSort<T, TKey, LargeStackAllocator.SequentialDataPair<TStackObj>>(source, getKey, comparer, stackObjSize * 2);
    }

    private static void DoSort<T, TKey, TStackObj>(Span<T> source, Func<T, TKey> getKey, IComparer<TKey> comparer)
        where TStackObj : struct
    {
        var stackobj = default(LargeStackAllocator.MemoryChunk<TKey, TStackObj>);
        var keys = MemoryMarshal.CreateSpan(ref stackobj.Head, source.Length);
        for (var i = 0; i < source.Length; ++i)
            keys[i] = getKey(source[i]);
        keys.Sort(source, comparer);
    }
}


public struct OrderByOnStreamState<TInput, TKey, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TInput, TResult>
        where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
{
    TProcessStream _processStream;
    Func<TInput, TKey> _getKey;
    IComparer<TKey> _keyComparer;
    int _index;

    public OrderByOnStreamState(in TProcessStream processStream, Func<TInput, TKey> getKey, IComparer<TKey> keyComparer) =>
        (_processStream, _getKey, _keyComparer, _index) = (processStream, getKey, keyComparer, 0);

    public TResult GetResult(ref StreamState<TInput> builder)
    {
        var source = builder.Current[.._index];
        OrderByHelpers.SortByKey(source, _getKey, _keyComparer);
        return Root<TInput>.Instance.Execute<TFinal, TResult, TProcessStream>(_processStream, source, 0);
    }

    public bool ProcessNext(ref StreamState<TInput> builder, in TInput input)
    {
        builder.Current[_index++] = input;
        return true;
    }
}
