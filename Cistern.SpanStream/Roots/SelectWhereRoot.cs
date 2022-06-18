using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectWhereRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TSource, TNext> _selector;
    readonly Func<TNext, bool> _predicate;

    public SelectWhereRoot(Func<TSource, TNext> selector, Func<TNext, bool> predicate) =>
        (_predicate, _selector) = (predicate, selector);

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        Iterator.SelectWhere(span, ref localCopy, _selector, _predicate);
        return localCopy.GetResult();
    }
}
