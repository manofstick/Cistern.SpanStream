using Cistern.Utils;
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

    int? IStreamNode<U>.TryGetSize(int sourceSize, out int upperBound) => Node.TryGetSize(sourceSize, out upperBound);

    TResult IStreamNode<U>.Execute<TRoot, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TCurrent, TResult, SelectStream<T, U, TCurrent, TResult, TProcessStream>>(in span, new(in processStream, Selector));
}

struct SelectStream<T, U, TCurrent, TResult, TProcessStream>
    : IProcessStream<T, TCurrent, TResult>
    where TProcessStream : struct, IProcessStream<U, TCurrent, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, U> _selector;

    public SelectStream(in TProcessStream nextProcessStream, Func<T, U> selector) =>
        (_next, _selector) = (nextProcessStream, selector);

    TResult IProcessStream<T, TCurrent, TResult>.GetResult(ref Builder<TCurrent> builder) => _next.GetResult(ref builder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, TCurrent>.ProcessNext(ref Builder<TCurrent> builder, in T input) =>
        _next.ProcessNext(ref builder, _selector(input));
}
