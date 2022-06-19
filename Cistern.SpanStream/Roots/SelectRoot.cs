using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;


public readonly struct SelectRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    public Func<TSource, TNext> Selector { get; }

    public SelectRoot(Func<TSource, TNext> selector) =>
        Selector = selector;

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Execute
        : IExecuteIterator<TSource, TNext, Func<TSource, TNext>>
    {
        TResult IExecuteIterator<TSource, TNext, Func<TSource, TNext>>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in Func<TSource, TNext> selector)
        {
            var localCopy = stream;
            Iterator.Select(ref builder, span, ref localCopy, selector);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TSource, TNext, TCurrent, TResult, TProcessStream, Func<TSource, TNext>, Execute>(0, ref span, in processStream, Selector);
    }
}
