using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public readonly struct WhereNode<T, NodeT>
    : INode<T>
    where NodeT : struct, INode<T>
{
    private NodeT Node { get; }
    private Func<T, bool> Filter { get; }

    public WhereNode(in NodeT nodeT, Func<T, bool> predicate) => (Node, Filter) = (nodeT, predicate);

    public TResult CreateViaPush<TRoot, TResult, TPushEnumerator>(in ReadOnlySpan<TRoot> span, in TPushEnumerator fenum)
        where TPushEnumerator : struct, IPushEnumerator<T> =>
        Node.CreateViaPush<TRoot, TResult, WhereFoward<T, TPushEnumerator>>(span, new WhereFoward<T, TPushEnumerator>(fenum, Filter));
}

struct WhereFoward<T, Next>
    : IPushEnumerator<T>
    where Next : struct, IPushEnumerator<T>
{
    Next _next;
    readonly Func<T, bool> _predicate;

    public WhereFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate) = (prior, predicate);

    public TResult GetResult<TResult>() => _next.GetResult<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ProcessNext(T input)
    {
        if (_predicate(input))
            return _next.ProcessNext(input);
        return true;
    }
}
