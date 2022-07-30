using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Transforms;

public /*readonly*/ struct Append<TInitial, TInput, TPriorNode>
    : IStreamNode<TInitial, TInput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    internal /*readonly*/ TPriorNode Node;
    public TInput Item { get; }

    private bool _appended;

    public Append(ref TPriorNode nodeT, TInput item) =>
        (Node, Item, _appended) = (nodeT, item, false);

    int? IStreamNode<TInitial, TInput>.TryGetSize(int sourceSize, out int upperBound)
    {
        var maybeSize = Node.TryGetSize(sourceSize, out upperBound);
        ++upperBound;
        return maybeSize + 1;
    }

    TResult IStreamNode<TInitial, TInput>.Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, AppendStream<TInput, TFinal, TResult, TProcessStream>, TContext>(new(in processStream, Item), in span, stackAllocationCount);

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TInput current)
    {
        if (Node.TryGetNext(ref state, out current))
            return true;

        if (!_appended)
        {
            current = Item;
            _appended = true;
            return true;
        }

        current = default!;
        return false;
    }
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
