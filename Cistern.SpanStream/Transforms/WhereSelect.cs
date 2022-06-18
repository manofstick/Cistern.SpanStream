using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct WhereSelect<T, U, NodeT>
    : IStreamNode<U>
    where NodeT : struct, IStreamNode<T>
{
    public readonly NodeT Node;
    private Func<T, bool> Predicate { get; }
    private Func<T, U> Selector { get; }

    public WhereSelect(in NodeT nodeT, Func<T, bool> predicate, Func<T, U> selector) =>
        (Node, Predicate, Selector) = (nodeT, predicate, selector);

    TResult IStreamNode<U>.Execute<TRoot, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TResult, WhereSelectStream<T, U, TResult, TProcessStream>>(in span, new(in processStream, Predicate, Selector));
}

struct WhereSelectStream<T, U, TResult, TProcessStream>
    : IProcessStream<T, TResult>
    where TProcessStream : struct, IProcessStream<U, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, bool> _predicate;
    readonly Func<T, U> _selector;

    public WhereSelectStream(in TProcessStream nextProcessStream, Func<T, bool> predicate, Func<T, U> selector) =>
        (_next, _predicate, _selector) = (nextProcessStream, predicate, selector);

    TResult IProcessStream<T, TResult>.GetResult() => _next.GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T>.ProcessNext(in T input) =>
        !_predicate(input) || _next.ProcessNext(_selector(input));
}
