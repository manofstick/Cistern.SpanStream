using Cistern.Utils;

namespace Cistern.SpanStream.Roots;


public readonly struct SelectRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    public Func<TInput, TOutput> Selector { get; }

    public SelectRoot(Func<TInput, TOutput> selector) =>
        Selector = selector;

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>
    {
        TResult StackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>.Execute<TFinal, TResult, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInput> span, in TProcessStream stream, in Func<TInput, TOutput> selector)
        {
            var localCopy = stream;
            Iterator.Select(ref state, in span, ref localCopy, selector);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, Func<TInput, TOutput>, Execute>(stackAllocationCount, in span, in processStream, Selector);
    }
}
