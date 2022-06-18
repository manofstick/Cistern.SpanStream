using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct Select<T, U, NodeT>
    : IStreamNode<U>
    where NodeT : struct, IStreamNode<T>
{
    public readonly NodeT Node;
    public Func<T, U> Selector { get; }

    public Select(in NodeT nodeT, Func<T, U> selector) =>
        (Node, Selector) = (nodeT, selector);

    TResult IStreamNode<U>.Execute<TRoot, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TResult, SelectStream<T, U, TResult, TProcessStream>>(in span, new(in processStream, Selector));
}

struct SelectStream<T, U, TResult, TProcessStream>
    : IProcessStream<T, TResult>
    where TProcessStream : struct, IProcessStream<U, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, U> _selector;

    public SelectStream(in TProcessStream nextProcessStream, Func<T, U> selector) =>
        (_next, _selector) = (nextProcessStream, selector);

    TResult IProcessStream<T, TResult>.GetResult() => _next.GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T>.ProcessNext(in T input) =>
        _next.ProcessNext(_selector(input));
}
