using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct Where<TCurrent, TPriorNode>
    : IStreamNode<TCurrent>
    where TPriorNode : struct, IStreamNode<TCurrent>
{
    public readonly TPriorNode Node;
    public Func<TCurrent, bool> Predicate { get; }

    public Where(in TPriorNode nodeT, Func<TCurrent, bool> predicate) =>
        (Node, Predicate) = (nodeT, predicate);

    int? IStreamNode<TCurrent>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return 0;
    }

    TResult IStreamNode<TCurrent>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> span, int? stackAllocationCount, in TProcessStream processStream) =>
        Node.Execute<TInitialDuplicate, TFinal, TResult, WhereStream<TCurrent, TFinal, TResult, TProcessStream>>(in span, stackAllocationCount, new(in processStream, Predicate));
}

struct WhereStream<TCurrent, TFinal, TResult, TProcessStream>
    : IProcessStream<TCurrent, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TCurrent, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<TCurrent, bool> _predicate;

    public WhereStream(in TProcessStream nextProcessStream, Func<TCurrent, bool> predicate) =>
        (_next, _predicate) = (nextProcessStream, predicate);

    TResult IProcessStream<TCurrent, TFinal, TResult>.GetResult(ref StreamState<TFinal> state) => _next.GetResult(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TCurrent, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TCurrent input) =>
        !_predicate(input) || _next.ProcessNext(ref state, input);
}
