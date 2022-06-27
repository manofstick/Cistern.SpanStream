using Cistern.Spanner.Roots;
using Cistern.Spanner.Terminators;
using Cistern.Spanner.Utils;
using Cistern.Utils;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Transforms;

public struct SingleKeyWithComparer<T>
    : IComparable<SingleKeyWithComparer<T>>
{
    internal T _item;
    internal IComparer<T> _comparer;

    public SingleKeyWithComparer(T item, IComparer<T> comparer) => (_item, _comparer) = (item, comparer);

    public int CompareTo(SingleKeyWithComparer<T> other) => _comparer.Compare(_item, other._item);
}

public struct CompositeKey<T, U>
    : IComparable<CompositeKey<T, U>>
{
    internal T Item1;
    internal U Item2;

    public int CompareTo(CompositeKey<T, U> other)
    {
        var c = Comparer<U>.Default.Compare(Item2, other.Item2);
        if (c != 0)
            return c;
        return Comparer<T>.Default.Compare(Item1, other.Item1);
    }
}
public struct CompositeKeyWithComparer<T, U>
    : IComparable<CompositeKeyWithComparer<T, U>>
{
    internal T Item1;
    internal IComparer<T> Comparer;
    internal U Item2;

    public int CompareTo(CompositeKeyWithComparer<T, U> other)
    {
        var c = Comparer<U>.Default.Compare(Item2, other.Item2);
        if (c != 0)
            return c;
        return Comparer.Compare(Item1, other.Item1);
    }
}

public interface IKeyFactory<TSource, T>
{
    void Create(in TSource source, out T key);
}

public struct SingleKeyFactory<TSource, T>
    : IKeyFactory<TSource, T>
{
    private readonly Func<TSource, T> _getT;

    public SingleKeyFactory(Func<TSource, T> getT) => _getT = getT;

    public void Create(in TSource source, out T key) => key = _getT(source);
}

public struct SingleKeyFactoryWithComparer<TSource, T>
    : IKeyFactory<TSource, SingleKeyWithComparer<T>>
{
    private readonly Func<TSource, T> _getT;
    private readonly IComparer<T> _comparer;

    public SingleKeyFactoryWithComparer(Func<TSource, T> getT, IComparer<T> comparer) => 
        (_getT, _comparer) = (getT, comparer);

    public void Create(in TSource source, out SingleKeyWithComparer<T> key)
    {
        key._item = _getT(source);
        key._comparer = _comparer;
    }
}

public struct CompositeKeyFactory<TSource, T, U, UFactory>
    : IKeyFactory<TSource, CompositeKey<T, U>>
    where UFactory : IKeyFactory<TSource, U>
{
    private readonly Func<TSource, T> _getT;
    UFactory _uFactory;

    public CompositeKeyFactory(Func<TSource, T> getT, UFactory uFactory) => (_getT, _uFactory) = (getT, uFactory);

    public void Create(in TSource source, out CompositeKey<T, U> key)
    {
        key.Item1 = _getT(source);
        _uFactory.Create(source, out key.Item2);
    }
}

public struct CompositeKeyFactoryWithComparer<TSource, T, U, UFactory>
    : IKeyFactory<TSource, CompositeKeyWithComparer<T, U>>
    where UFactory : IKeyFactory<TSource, U>
{
    private readonly Func<TSource, T> _getT;
    private readonly IComparer<T> _comparer;
    UFactory _uFactory;

    public CompositeKeyFactoryWithComparer(Func<TSource, T> getT, IComparer<T> comparer, UFactory uFactory) =>
        (_getT, _comparer, _uFactory) = (getT, comparer, uFactory);

    public void Create(in TSource source, out CompositeKeyWithComparer<T, U> key)
    {
        key.Item1 = _getT(source);
        key.Comparer = _comparer;
        _uFactory.Create(source, out key.Item2);
    }
}


public /*readonly*/ struct OrderBy<TInitial, TInput, TKey, TKeyFactory, TPriorNode>
    : IStreamNode<TInitial, TInput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
    where TKeyFactory : struct, IKeyFactory<TInput, TKey>
{
    internal /*readonly*/ TPriorNode Node;

    internal TKeyFactory _keyFactory;

    private int _stackElementCount;
    private ArrayPool<TInput>? _maybeArrayPool;

    private int _index;
    private TInput[] _ordered;

    public OrderBy(ref TPriorNode nodeT, TKeyFactory keyFactory, int? stackElementCount, ArrayPool<TInput>? maybeArrayPool)
    {
        Node = nodeT;
        _keyFactory = keyFactory;
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
        var _ = Node.TryGetSize(span.Length, out var upperBound);
        if (upperBound <= _stackElementCount)
        {
            return Node.Execute<TInput, TResult, OrderByOnStreamState<TInput, TKey, TKeyFactory, TFinal, TResult, TProcessStream>>(new(in processStream, _keyFactory), span, upperBound);
        }
        else
        {
            var orderedArray = CreateOrderByArray(span);
            return Root<TInput>.Instance.Execute<TFinal, TResult, TProcessStream>(processStream, orderedArray.ToReadOnlySpan(), 0);
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
            return Root<TInput>.Instance.Execute<TKey, TInput[], OrderByArrayOnStreamState<TInput, TKey, TKeyFactory>>(new(buffer, _keyFactory), buffer.AsSpan(), buffer.Length);
        else
        {
            var keyFactory = _keyFactory;
            var select = new SelectRoot<TInput, TKey>(input => { keyFactory.Create(input, out var key); return key; });
            var keys = ToArray<TKey>.Execute(buffer.ToReadOnlySpan(), ref select, _stackElementCount, null);
            Array.Sort(keys, buffer);
            return buffer;
        }
    }
}

public struct OrderByArrayOnStreamState<TInput, TKey, TKeyFactory>
    : IProcessStream<TInput, TKey, TInput[]>
    where TKeyFactory : IKeyFactory<TInput, TKey>
{
    TInput[] _toSort;
    TKeyFactory _keyFactory;
    int _index;

    public OrderByArrayOnStreamState(TInput[] toSort, TKeyFactory keyFactory) =>
        (_toSort, _keyFactory, _index) = (toSort, keyFactory, 0);

    public TInput[] GetResult(ref StreamState<TKey> builder)
    {
        builder.Current[.._index].Sort(_toSort.AsSpan());
        return _toSort;
    }

    public bool ProcessNext(ref StreamState<TKey> builder, in TInput input)
    {
        _keyFactory.Create(input, out builder.Current[_index++]);
        return true;
    }
}

static class OrderByHelpers
{
    public static void SortByKey<T, TKey, TKeyFactory>(Span<T> source, TKeyFactory keyFactory)
        where TKeyFactory : IKeyFactory<T, TKey>
    {
        BuildStackToSort<T, TKey, TKeyFactory, LargeStackAllocator.BufferStorage<TKey>>(source, keyFactory, LargeStackAllocator.BufferStorage<TKey>.NumberOfElements);
    }

    private static void BuildStackToSort<T, TKey, TKeyFactory, TStackObj>(Span<T> source, TKeyFactory keyFactory, int stackObjSize)
        where TStackObj : struct
        where TKeyFactory : IKeyFactory<T, TKey>
    {
        if (stackObjSize >= source.Length)
            DoSort<T, TKey, TKeyFactory, TStackObj>(source, keyFactory);
        else
            BuildStackToSort<T, TKey, TKeyFactory, LargeStackAllocator.SequentialDataPair<TStackObj>>(source, keyFactory, stackObjSize * 2);
    }

    private static void DoSort<T, TKey, TKeyFactory, TStackObj>(Span<T> source, TKeyFactory keyFactory)
        where TStackObj : struct
        where TKeyFactory : IKeyFactory<T, TKey>
    {
        var stackobj = default(LargeStackAllocator.MemoryChunk<TKey, TStackObj>);
        var keys = MemoryMarshal.CreateSpan(ref stackobj.Head, source.Length);
        for (var i = 0; i < source.Length; ++i)
            keyFactory.Create(source[i], out keys[i]);
        keys.Sort(source);
    }
}

public struct OrderByOnStreamState<TInput, TKey, TKeyFactory, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TInput, TResult>
        where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
        where TKeyFactory : struct, IKeyFactory<TInput, TKey>
{
    TProcessStream _processStream;
    TKeyFactory _keyFactory;
    int _index;

    public OrderByOnStreamState(in TProcessStream processStream, TKeyFactory keyFactory) =>
        (_processStream, _keyFactory, _index) = (processStream, keyFactory, 0);

    public TResult GetResult(ref StreamState<TInput> builder)
    {
        var source = builder.Current[.._index];
        OrderByHelpers.SortByKey<TInput, TKey, TKeyFactory>(source, _keyFactory);
        return Root<TInput>.Instance.Execute<TFinal, TResult, TProcessStream>(_processStream, source, 0);
    }

    public bool ProcessNext(ref StreamState<TInput> builder, in TInput input)
    {
        builder.Current[_index++] = input;
        return true;
    }
}
