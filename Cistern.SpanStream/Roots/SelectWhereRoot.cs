using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectWhereRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    readonly Func<TInput, TOutput> _selector;
    readonly Func<TOutput, bool> _predicate;

    public SelectWhereRoot(Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>
    {
        TResult StackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> state, in ReadOnlySpan<TInput> span, in TProcessStream stream, in (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate) args)
        {
            var localCopy = stream;
            Iterator.SelectWhere(ref state, in span, ref localCopy, args.Selector, args.Predicate);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate), Execute>(stackAllocationCount, in span, in processStream, (_selector, _predicate));
    }
}
