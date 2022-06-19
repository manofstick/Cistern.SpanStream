using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<TSource>
    : IStreamNode<TSource>
{
    public readonly Func<TSource, bool> Predicate;

    public WhereRoot(Func<TSource, bool> predicate) => Predicate = predicate;

    int? IStreamNode<TSource>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TSource, TSource, Func<TSource, bool>>
    {
        TResult IExecuteIterator<TSource, TSource, Func<TSource, bool>>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in Func<TSource, bool> predicate)
        {
            var localCopy = stream;
            Iterator.Where(ref builder, span, ref localCopy, predicate);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TSource>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TSource, TSource, TCurrent, TResult, TProcessStream, Func<TSource, bool>, Execute>(0, ref span, in processStream, Predicate);
    }
}
