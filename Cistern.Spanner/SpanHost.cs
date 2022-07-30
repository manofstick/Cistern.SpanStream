namespace Cistern.Spanner;

public /*readonly*/ ref struct SpanHost<TInitial, TCurrent, TStreamNode, TContext>
    where TStreamNode : struct, IStreamNode<TInitial, TCurrent>
{
    internal /*public readonly*/ ReadOnlySpan<TInitial> Span;
    internal /*public readonly*/ TStreamNode Node;

    public SpanHost(in ReadOnlySpan<TInitial> span, in TStreamNode node)
    {
        Span = span;
        Node = node;
    }

    public EnumeratorHost<TInitial, TCurrent, TStreamNode> GetEnumerator() => new(in Span, in Node);

    public int? TryGetSize(out int upperBound) =>
        Node.TryGetSize(Span.Length, out upperBound);

    public SpanHost<TInitial, TCurrent, TStreamNode, TNewContext> SetContext<TNewContext>()
        where TNewContext : struct => new (in Span, in Node);

    public TResult Execute<TResult, TProcessStream>(TProcessStream finalNode, int? stackAllocationCount = null)
        where TProcessStream : struct, IProcessStream<TCurrent, TCurrent, TResult> =>
        Node.Execute<TCurrent, TResult, TProcessStream, TContext>(in finalNode, in Span, stackAllocationCount);
}

public ref struct EnumeratorState<T>
{
    internal ReadOnlySpan<T> Span;
    internal int Index;
}

public ref struct EnumeratorHost<TInitial, TCurrent, TStreamNode>
    where TStreamNode : struct, IStreamNode<TInitial, TCurrent>
{
    TStreamNode _node;

    EnumeratorState<TInitial> _state;
    TCurrent _current;

    internal EnumeratorHost(in ReadOnlySpan<TInitial> span, in TStreamNode node)
    {
        _node = node;
        _state = new() { Span = span };
        _current = default!;
    }

    public TCurrent Current => _current;

    public bool MoveNext() => _node.TryGetNext(ref _state, out _current);
}