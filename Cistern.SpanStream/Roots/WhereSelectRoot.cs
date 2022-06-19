using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereSelectRoot<TInput, TOutput>
    : IStreamNode<TOutput>
{
    readonly Func<TInput, bool> _predicate;
    readonly Func<TInput, TOutput> _selector;

    public WhereSelectRoot(Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>
    {
        TResult StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>.Execute<TTerminatorState, TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent, TTerminatorState> state, ref Span<TInput> span, in TProcessStream stream, in (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector) args)
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref state, span, ref localCopy, args.Predicate, args.Selector);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TOutput>.Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, int? stackAllocationCount, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInput>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TTerminatorState, TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector), Execute>(stackAllocationCount, ref span, in processStream, (_predicate, _selector));
    }
}
