using Cistern.SpanStream.Utils;

namespace Cistern.SpanStream;

public readonly struct RootNode<TSource>
    : IStreamNode<TSource>
{
    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        var localCopy = processStream;
        foreach (var item in span)
            localCopy.ProcessNext(item);
        return localCopy.GetResult();
    }
}
