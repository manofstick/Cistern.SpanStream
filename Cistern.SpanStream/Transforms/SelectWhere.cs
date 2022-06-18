using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct SelectWhere<T, U, NodeT>
    : IStreamNode<U>
    where NodeT : struct, IStreamNode<T>
{
    public readonly NodeT Node;
    private Func<T, U> Selector { get; }
    private Func<U, bool> Predicate { get; }

    public SelectWhere(in NodeT nodeT, Func<T, U> selector, Func<U, bool> predicate) =>
        (Node, Selector, Predicate) = (nodeT, selector, predicate);

    TResult IStreamNode<U>.Execute<TRoot, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TResult, SelectWhereStream<T, U, TResult, TProcessStream>>(in span, new(in processStream, Selector, Predicate));
}

struct SelectWhereStream<T, U, TResult, TProcessStream>
    : IProcessStream<T, TResult>
    where TProcessStream : struct, IProcessStream<U, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, U> _selector;
    readonly Func<U, bool> _predicate;

    public SelectWhereStream(in TProcessStream nextProcessStream, Func<T, U> selector, Func<U, bool> predicate) =>
        (_next, _selector, _predicate) = (nextProcessStream, selector, predicate);

    TResult IProcessStream<T, TResult>.GetResult() => _next.GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T>.ProcessNext(in T input)
    {
        var u = _selector(input);
        if (_predicate(u))
            return _next.ProcessNext(u);
        return true;
    }
}
