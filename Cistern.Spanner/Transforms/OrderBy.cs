using Cistern.Spanner.Roots;
using Cistern.Spanner.Terminators;
using Cistern.Spanner.Utils;
using Cistern.Utils;
using System.Buffers;

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
        _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        _stackElementCount = stackElementCount ?? 0;
        _maybeArrayPool = maybeArrayPool;
        _ordered = UninitializedArrayStandIn<TInput>.Instance;
        _index = int.MaxValue;
    }

    int? IStreamNode<TInitial, TInput>.TryGetSize(int sourceSize, out int upperBound) =>
        Node.TryGetSize(sourceSize, out upperBound);

    public TResult Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
        where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
    {
        throw new NotImplementedException();
        //var _ = Node.TryGetSize(span.Length, out var upperBound);
        //if (upperBound <= _stackElementCount)
        //{
        //    return Node.Execute<TInput, TResult, OrderByOnStreamState<TInput, TFinal, TResult, TProcessStream>>(new(in processStream, upperBound), span, upperBound);
        //}
        //else
        //{
        //    var reversedArray = CreateOrderBydArray(span);
        //    return Root<TInput>.Instance.Execute<TFinal, TResult, TProcessStream>(processStream, reversedArray.ToReadOnlySpan(), 0);
        //}
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
