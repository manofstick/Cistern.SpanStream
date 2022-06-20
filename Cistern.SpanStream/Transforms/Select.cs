using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct Select<TInitial, TInput, TOutput, TPriorNode>
    : IStreamNode<TInitial, TOutput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    public readonly TPriorNode Node;
    public Func<TInput, TOutput> Selector { get; }

    public Select(in TPriorNode nodeT, Func<TInput, TOutput> selector) =>
        (Node, Selector) = (nodeT, selector);

    int? IStreamNode<TInitial, TOutput>.TryGetSize(int sourceSize, out int upperBound) => Node.TryGetSize(sourceSize, out upperBound);

    TResult IStreamNode<TInitial, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitial> span, int? stackAllocationCount, in TProcessStream processStream) =>
        Node.Execute<TFinal, TResult, SelectStream<TInput, TOutput, TFinal, TResult, TProcessStream>>(in span, stackAllocationCount, new(in processStream, Selector));
}

struct SelectStream<TInput, TOutput, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<TInput, TOutput> _selector;

    public SelectStream(in TProcessStream nextProcessStream, Func<TInput, TOutput> selector) =>
        (_next, _selector) = (nextProcessStream, selector);

    TResult IProcessStream<TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal> state) => _next.GetResult(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TInput, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TInput input) =>
        _next.ProcessNext(ref state, _selector(input));
}
