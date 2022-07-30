using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Transforms;

public /*readonly*/ struct SelectWhere<TInitial, TInput, TOutput, TPriorNode>
    : IStreamNode<TInitial, TOutput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    internal /*readonly*/ TPriorNode Node;
    private Func<TInput, TOutput> Selector { get; }
    private Func<TOutput, bool> Predicate { get; }

    public SelectWhere(ref TPriorNode nodeT, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (Node, Selector, Predicate) = (nodeT, selector, predicate);

    int? IStreamNode<TInitial, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return null;
    }

    TResult IStreamNode<TInitial, TOutput>.Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, SelectWhereStream<TInput, TOutput, TFinal, TResult, TProcessStream>, TContext>(new(in processStream, Selector, Predicate), in span, stackAllocationCount);

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TOutput current)
    {
        while (Node.TryGetNext(ref state, out var item))
        {
            current = Selector(item);
            if (Predicate(current))
                return true;
        }
        current = default!;
        return false;
    }
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
