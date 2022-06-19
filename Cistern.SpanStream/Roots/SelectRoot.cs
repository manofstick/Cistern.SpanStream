using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;


public readonly struct SelectRoot<TInput, TOutput>
    : IStreamNode<TOutput>
{
    public Func<TInput, TOutput> Selector { get; }

    public SelectRoot(Func<TInput, TOutput> selector) =>
        Selector = selector;

    int? IStreamNode<TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>
    {
        TResult StackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>.Execute<TTerminatorState, TFinal, TResult, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, ref Span<TInput> span, in TProcessStream stream, in Func<TInput, TOutput> selector)
        {
            var localCopy = stream;
            Iterator.Select(ref state, span, ref localCopy, selector);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TOutput>.Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, int? stackAllocationCount, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInput>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TTerminatorState, TInput, TOutput, TFinal, TResult, TProcessStream, Func<TInput, TOutput>, Execute>(stackAllocationCount, ref span, in processStream, Selector);
    }
}
