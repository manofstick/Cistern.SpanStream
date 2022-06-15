namespace Cistern.SpanStream;

public readonly ref struct SpanHost<TRoot, TCurrent, TNode>
    where TNode : struct, INode<TCurrent>
{
    public ReadOnlySpan<TRoot> Span { get; }
    public TNode Node { get; }

    public SpanHost(in ReadOnlySpan<TRoot> span, in TNode node)
    {
        Span = span;
        Node = node;
    }

    public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
        where TPushEnumerator : struct, IPushEnumerator<TCurrent> =>
        Node.CreateViaPush<TRoot, TResult, TPushEnumerator>(Span, fenum);
}
