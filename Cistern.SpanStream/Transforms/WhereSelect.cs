using Cistern.Utils;
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

    int? IStreamNode<U>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return 0;
    }

    TResult IStreamNode<U>.Execute<TRoot, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TCurrent, TResult, WhereSelectStream<T, U, TCurrent, TResult, TProcessStream>>(in span, new(in processStream, Predicate, Selector));
}

struct WhereSelectStream<T, U, TCurrent, TResult, TProcessStream>
    : IProcessStream<T, TCurrent, TResult>
    where TProcessStream : struct, IProcessStream<U, TCurrent, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, bool> _predicate;
    readonly Func<T, U> _selector;

    public WhereSelectStream(in TProcessStream nextProcessStream, Func<T, bool> predicate, Func<T, U> selector) =>
        (_next, _predicate, _selector) = (nextProcessStream, predicate, selector);

    TResult IProcessStream<T, TCurrent, TResult>.GetResult(ref Builder<TCurrent> builder) => _next.GetResult(ref builder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, TCurrent>.ProcessNext(ref Builder<TCurrent> builder, in T input) =>
        !_predicate(input) || _next.ProcessNext(ref builder, _selector(input));
}
