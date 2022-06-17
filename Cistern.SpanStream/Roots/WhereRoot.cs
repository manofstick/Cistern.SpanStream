using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<TSource>
    : IStreamNode<TSource>
{
    readonly Func<TSource, bool> _predicate;

    public WhereRoot(Func<TSource, bool> predicate) => _predicate = predicate;

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        Iterator.Run(span, ref localCopy, _predicate);
        return localCopy.GetResult();
    }
}
