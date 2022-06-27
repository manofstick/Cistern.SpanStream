using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Transforms;

public /*readonly*/ struct WhereSelect<TInitial, TInput, TOutput, TPriorNode>
    : IStreamNode<TInitial, TOutput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
{
    internal /*readonly*/ TPriorNode Node;
    private Func<TInput, bool> Predicate { get; }
    private Func<TInput, TOutput> Selector { get; }

    public WhereSelect(ref TPriorNode nodeT, Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (Node, Predicate, Selector) = (nodeT, predicate, selector);

    int? IStreamNode<TInitial, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        Node.TryGetSize(sourceSize, out upperBound);
        return null;
    }

    TResult IStreamNode<TInitial, TOutput>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, WhereSelectStream<TInput, TOutput, TFinal, TResult, TProcessStream>>(new(in processStream, Predicate, Selector), in span, stackAllocationCount);

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TOutput current)
    {
        while (Node.TryGetNext(ref state, out var item))
        {
            if (Predicate(item))
            {
                current = Selector(item);
                return true;
            }
        }
        current = default!;
        return false;
    }
}

struct WhereSelectStream<TInput, TOutput, TFinal, TResult, TProcessStream>
    : IProcessStream<TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
{
    /* can't be readonly */ TProcessStream _next;
    readonly Func<TInput, bool> _predicate;
    readonly Func<TInput, TOutput> _selector;

    public WhereSelectStream(in TProcessStream nextProcessStream, Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (_next, _predicate, _selector) = (nextProcessStream, predicate, selector);

    TResult IProcessStream<TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal> state) => _next.GetResult(ref state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TInput, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TInput input) =>
        !_predicate(input) || _next.ProcessNext(ref state, _selector(input));
}
