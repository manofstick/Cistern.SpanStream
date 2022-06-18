using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    public Func<TSource, TNext> Selector { get; }

    public SelectRoot(Func<TSource, TNext> selector) =>
        (Selector) = (selector);

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        Builder<TCurrent>.MemoryChunk memoryChunk = new ();
        var builder = new Builder<TCurrent>(null, memoryChunk.GetBufferofBuffers(), memoryChunk.GetBufferOfItems(), null);
        var localCopy = processStream;
        Iterator.Select(ref builder, span, ref localCopy, Selector);
        return localCopy.GetResult(ref builder);
    }
}
