using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectWhereRoot<TSource, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TSource, TNext> _selector;
    readonly Func<TNext, bool> _predicate;

    public SelectWhereRoot(Func<TSource, TNext> selector, Func<TNext, bool> predicate) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TSource, TNext, (Func<TSource, TNext> Selector, Func<TNext, bool> Predicate)>
    {
        TResult IExecuteIterator<TSource, TNext, (Func<TSource, TNext> Selector, Func<TNext, bool> Predicate)>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in (Func<TSource, TNext> Selector, Func<TNext, bool> Predicate) state)
        {
            var localCopy = stream;
            Iterator.SelectWhere(ref builder, span, ref localCopy, state.Selector, state.Predicate);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TSource, TNext, TCurrent, TResult, TProcessStream, (Func<TSource, TNext> Selector, Func<TNext, bool> Predicate), Execute>(0, ref span, in processStream, (_selector, _predicate));
    }
}
