using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TSource, TNext> _selector;

    public SelectRoot(Func<TSource, TNext> selector) =>
        (_selector) = (selector);

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        Iterator.Select(span, ref localCopy, _selector);
        return localCopy.GetResult();
    }
}
