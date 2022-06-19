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
        : IExecuteIterator<TInput, TOutput, Func<TInput, TOutput>>
    {
        TResult IExecuteIterator<TInput, TOutput, Func<TInput, TOutput>>.Execute<TFinal, TResult, TProcessStream>(ref StreamState<TFinal> state, ref Span<TInput> span, in TProcessStream stream, in Func<TInput, TOutput> selector)
        {
            var localCopy = stream;
            Iterator.Select(ref state, span, ref localCopy, selector);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TOutput>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInput>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, Func<TInput, TOutput>, Execute>(0, ref span, in processStream, Selector);
    }
}
