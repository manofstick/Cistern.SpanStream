using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereSelectRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TSource, bool> _predicate;
    readonly Func<TSource, TNext> _selector;

    public WhereSelectRoot(Func<TSource, bool> predicate, Func<TSource, TNext> selector) =>
        (_predicate, _selector) = (predicate, selector);

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        Builder<TCurrent>.MemoryChunk memoryChunk = new();
        var builder = new Builder<TCurrent>(null, memoryChunk.GetBufferofBuffers(), memoryChunk.GetBufferOfItems(), null);
        var localCopy = processStream;
        Iterator.WhereSelect(ref builder, span, ref localCopy, _predicate, _selector);
        return localCopy.GetResult(ref builder);
    }
}
