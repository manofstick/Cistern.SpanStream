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

    struct Null { }

    struct Execute
        : IExecuteIterator<TSource, TSource, Null>
    {
        TResult IExecuteIterator<TSource, TSource, Null>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in Null selector)
        {
            var localCopy = stream;
            Iterator.Vanilla(ref builder, span, ref localCopy);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TSource, TSource, TCurrent, TResult, TProcessStream, Null, Execute>(0, ref span, in processStream, default);
    }
}
