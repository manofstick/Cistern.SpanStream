using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct SelectWhere<TInput, TOutput, TPriorNode>
    : IStreamNode<TOutput>
    where TPriorNode : struct, IStreamNode<TInput>
{
    public readonly TPriorNode Node;
    private Func<TInput, TOutput> Selector { get; }
    private Func<TOutput, bool> Predicate { get; }

    public SelectWhere(in TPriorNode nodeT, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (Node, Selector, Predicate) = (nodeT, selector, predicate);

    int? IStreamNode<TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return 0;
    }

    TResult IStreamNode<TOutput>.Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> span, int? stackAllocationCount, in TProcessStream processStream) =>
        Node.Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, SelectWhereStream<TTerminatorState, TInput, TOutput, TFinal, TResult, TProcessStream>>(in span, stackAllocationCount, new(in processStream, Selector, Predicate));
}

struct SelectWhereStream<TTerminatorState, TInput, TOutput, TFinal, TResult, TProcessStream>
    : IProcessStream<TTerminatorState, TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TTerminatorState, TOutput, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<TInput, TOutput> _selector;
    readonly Func<TOutput, bool> _predicate;

    public SelectWhereStream(in TProcessStream nextProcessStream, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (_next, _selector, _predicate) = (nextProcessStream, selector, predicate);

    TResult IProcessStream<TTerminatorState, TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal, TTerminatorState> state) => _next.GetResult(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TTerminatorState, TInput, TFinal>.ProcessNext(ref StreamState<TFinal, TTerminatorState> state, in TInput input)
    {
        var u = _selector(input);
        if (_predicate(u))
            return _next.ProcessNext(ref state, u);
        return true;
    }
}
