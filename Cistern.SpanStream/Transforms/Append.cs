using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Transforms;

public readonly struct Append<TInitial, TInput, TPriorNode>
    : IStreamNode<TInitial, TInput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    public readonly TPriorNode Node;
    public TInput Item { get; }

    public Append(in TPriorNode nodeT, TInput item) =>
        (Node, Item) = (nodeT, item);

    int? IStreamNode<TInitial, TInput>.TryGetSize(int sourceSize, out int upperBound)
    {
        var maybeSize = Node.TryGetSize(sourceSize, out upperBound);
        ++upperBound;
        return maybeSize + 1;
    }

    TResult IStreamNode<TInitial, TInput>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, AppendStream<TInput, TFinal, TResult, TProcessStream>>(new(in processStream, Item), in span, stackAllocationCount);
}

struct AppendStream<TInput, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly TInput _item;

    public AppendStream(in TProcessStream nextProcessStream, TInput item) =>
        (_next, _item) = (nextProcessStream, item);

    TResult IProcessStream<TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal> state)
    {
        _next.ProcessNext(ref state, _item);
        return _next.GetResult(ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TInput, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TInput input) =>
        _next.ProcessNext(ref state, input);
}
