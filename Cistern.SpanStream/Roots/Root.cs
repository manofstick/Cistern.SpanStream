using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct Root<TSource>
    : IStreamNode<TSource>
{
    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        Iterator.Vanilla(span, ref localCopy);
        return localCopy.GetResult();
    }
}
