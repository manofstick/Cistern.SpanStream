using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct SelectWhere<TInitial, TInput, TOutput, TPriorNode>
    : IStreamNode<TInitial, TOutput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    public readonly TPriorNode Node;
    private Func<TInput, TOutput> Selector { get; }
    private Func<TOutput, bool> Predicate { get; }

    public SelectWhere(in TPriorNode nodeT, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (Node, Selector, Predicate) = (nodeT, selector, predicate);

    int? IStreamNode<TInitial, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return 0;
    }

    TResult IStreamNode<TInitial, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitial> span, int? stackAllocationCount, in TProcessStream processStream) =>
        Node.Execute<TFinal, TResult, SelectWhereStream<TInput, TOutput, TFinal, TResult, TProcessStream>>(in span, stackAllocationCount, new(in processStream, Selector, Predicate));
}

struct SelectWhereStream<TInput, TOutput, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<TInput, TOutput> _selector;
    readonly Func<TOutput, bool> _predicate;

    public SelectWhereStream(in TProcessStream nextProcessStream, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (_next, _selector, _predicate) = (nextProcessStream, selector, predicate);

    TResult IProcessStream<TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal> state) => _next.GetResult(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TInput, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TInput input)
    {
        var u = _selector(input);
        if (_predicate(u))
            return _next.ProcessNext(ref state, u);
        return true;
    }
}
