using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct Where<T, NodeT>
    : IStreamNode<T>
    where NodeT : struct, IStreamNode<T>
{
    public readonly NodeT Node;
    public Func<T, bool> Predicate { get; }

    public Where(in NodeT nodeT, Func<T, bool> predicate) =>
        (Node, Predicate) = (nodeT, predicate);

    int? IStreamNode<T>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return 0;
    }

    TResult IStreamNode<T>.Execute<TRoot, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TRoot> span, in TProcessStream processStream) =>
        Node.Execute<TRoot, TCurrent, TResult, WhereStream<T, TCurrent, TResult, TProcessStream>>(in span, new(in processStream, Predicate));
}

struct WhereStream<T, TCurrent, TResult, TProcessStream>
    : IProcessStream<T, TCurrent, TResult>
    where TProcessStream : struct, IProcessStream<T, TCurrent, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<T, bool> _predicate;

    public WhereStream(in TProcessStream nextProcessStream, Func<T, bool> predicate) =>
        (_next, _predicate) = (nextProcessStream, predicate);

    TResult IProcessStream<T, TCurrent, TResult>.GetResult(ref Builder<TCurrent> builder) => _next.GetResult(ref builder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, TCurrent>.ProcessNext(ref Builder<TCurrent> builder, in T input) =>
        !_predicate(input) || _next.ProcessNext(ref builder, input);
}
