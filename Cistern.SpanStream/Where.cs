using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public readonly struct WhereNode<T, NodeT>
    : IStreamNode<T>
    where NodeT : struct, IStreamNode<T>
{
    private readonly NodeT Node;
    private readonly Func<T, bool> Filter;

    public WhereNode(in NodeT nodeT, Func<T, bool> predicate) => (Node, Filter) = (nodeT, predicate);

    TResult IStreamNode<T>.Execute<TRoot, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TResult, WhereFoward<T, TResult, TProcessStream>>(in span, new (in processStream, Filter));
}

struct WhereFoward<T, TResult, TProcessStream>
    : IProcessStream<T, TResult>
    where TProcessStream : struct, IProcessStream<T, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, bool> _predicate;

    public WhereFoward(in TProcessStream nextProcessStream, Func<T, bool> predicate) =>
        (_next, _predicate) = (nextProcessStream, predicate);

    TResult IProcessStream<T, TResult>.GetResult() => _next.GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, TResult>.ProcessNext(T input) =>
        !_predicate(input) || _next.ProcessNext(input);
}
