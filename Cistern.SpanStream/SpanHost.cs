namespace Cistern.SpanStream;

public readonly ref struct SpanHost<TSource, TCurrent, TStreamNode>
    where TStreamNode : struct, IStreamNode<TCurrent>
{
    public readonly ReadOnlySpan<TSource> Span;
    public readonly TStreamNode Node;

    public SpanHost(in ReadOnlySpan<TSource> span, in TStreamNode node)
    {
        Span = span;
        Node = node;
    }

    public TResult Execute<TResult, TProcessStream>(in TProcessStream processStream)
        where TProcessStream : struct, IProcessStream<TCurrent, TCurrent, TResult> =>
        Node.Execute<TSource, TCurrent, TResult, TProcessStream>(Span, processStream);
}
