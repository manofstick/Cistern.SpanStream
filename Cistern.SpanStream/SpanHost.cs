namespace Cistern.SpanStream;

public readonly ref struct SpanHost<TInitial, TCurrent, TStreamNode>
    where TStreamNode : struct, IStreamNode<TCurrent>
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

    public TResult Execute<TTerminatorState, TResult, TProcessStream>(in TProcessStream finalNode, int? stackAllocationCount = null)
        where TProcessStream : struct, IProcessStream<TTerminatorState, TCurrent, TCurrent, TResult> =>
        Node.Execute<TTerminatorState, TInitial, TCurrent, TResult, TProcessStream>(Span, stackAllocationCount, finalNode);
}
