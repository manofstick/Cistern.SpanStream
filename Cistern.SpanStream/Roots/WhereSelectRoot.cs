using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereSelectRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    readonly Func<TInput, bool> _predicate;
    readonly Func<TInput, TOutput> _selector;

    public WhereSelectRoot(Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>
    {
        TResult StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> state, in ReadOnlySpan<TInput> span, in TProcessStream stream, in (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector) args)
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref state, in span, ref localCopy, args.Predicate, args.Selector);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector), Execute>(stackAllocationCount, in span, in processStream, (_predicate, _selector));
    }
}
