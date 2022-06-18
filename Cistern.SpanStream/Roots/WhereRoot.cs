using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<TSource>
    : IStreamNode<TSource>
{
    public readonly Func<TSource, bool> Predicate;

    public WhereRoot(Func<TSource, bool> predicate) => Predicate = predicate;

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        Builder<TCurrent>.MemoryChunk memoryChunk = new();
        var builder = new Builder<TCurrent>(null, memoryChunk.GetBufferofBuffers(), memoryChunk.GetBufferOfItems(), null);
        var localCopy = processStream;
        Iterator.Where(ref builder, span, ref localCopy, Predicate);
        return localCopy.GetResult(ref builder);
    }
}
