using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct Root<TSource>
    : IStreamNode<TSource>
{
    int? IStreamNode<TSource>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var builder = new Builder<TCurrent>();
        var localCopy = processStream;
        Iterator.Vanilla(ref builder, span, ref localCopy);
        return localCopy.GetResult(ref builder);
    }
}
