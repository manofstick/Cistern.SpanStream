using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<TSource>
    : IStreamNode<TSource>
{
    public readonly Func<TSource, bool> Predicate;

    public WhereRoot(Func<TSource, bool> predicate) => Predicate = predicate;

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        Iterator.Where(span, ref localCopy, Predicate);
        return localCopy.GetResult();
    }
}
