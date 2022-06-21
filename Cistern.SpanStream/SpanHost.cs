namespace Cistern.SpanStream;

public readonly ref struct SpanHost<TInitial, TCurrent, TStreamNode>
    where TStreamNode : struct, IStreamNode<TInitial, TCurrent>
{
    public readonly ReadOnlySpan<TInitial> Span;
    public readonly TStreamNode Node;

    public SpanHost(in ReadOnlySpan<TInitial> span, in TStreamNode node)
    {
        Span = span;
        Node = node;
    }

    public int? TryGetSize(out int upperBound) =>
        Node.TryGetSize(Span.Length, out upperBound);

    public TResult Execute<TResult, TProcessStream>(in TProcessStream finalNode, int? stackAllocationCount = null)
        where TProcessStream : struct, IProcessStream<TCurrent, TCurrent, TResult> =>
        Node.Execute<TCurrent, TResult, TProcessStream>(in finalNode, in Span, stackAllocationCount);
}
