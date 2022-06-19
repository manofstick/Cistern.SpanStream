using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;


public readonly struct SelectRoot<TInitial, TNext>
    : IStreamNode<TNext>
{
    public Func<TInitial, TNext> Selector { get; }

    public SelectRoot(Func<TInitial, TNext> selector) =>
        Selector = selector;

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Execute
        : IExecuteIterator<TInitial, TNext, Func<TInitial, TNext>>
    {
        TResult IExecuteIterator<TInitial, TNext, Func<TInitial, TNext>>.Execute<TFinal, TResult, TProcessStream>(ref Builder<TFinal> builder, ref Span<TInitial> span, in TProcessStream stream, in Func<TInitial, TNext> selector)
        {
            var localCopy = stream;
            Iterator.Select(ref builder, span, ref localCopy, selector);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInitial>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInitial, TNext, TFinal, TResult, TProcessStream, Func<TInitial, TNext>, Execute>(0, ref span, in processStream, Selector);
    }
}
