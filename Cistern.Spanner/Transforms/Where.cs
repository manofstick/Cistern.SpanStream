using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Transforms;

public /*readonly*/ struct Where<TInitial, TCurrent, TPriorNode>
    : IStreamNode<TInitial, TCurrent>
    where TPriorNode : struct, IStreamNode<TInitial, TCurrent>
{
    internal /*readonly*/ TPriorNode Node;
    public Func<TCurrent, bool> Predicate { get; }

    public Where(ref TPriorNode nodeT, Func<TCurrent, bool> predicate) =>
        (Node, Predicate) = (nodeT, predicate);

    int? IStreamNode<TInitial, TCurrent>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return null;
    }

    TResult IStreamNode<TInitial, TCurrent>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, WhereStream<TCurrent, TFinal, TResult, TProcessStream>>(new(in processStream, Predicate), in span, stackAllocationCount);

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TCurrent current)
    {
        while (Node.TryGetNext(ref state, out current))
        {
            if (Predicate(current))
                return true;
        }
        current = default!;
        return false;
    }
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
