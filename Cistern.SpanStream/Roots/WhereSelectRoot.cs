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

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TSource, TNext, (Func<TSource, bool> Predicate, Func<TSource, TNext> Selector)>
    {
        TResult IExecuteIterator<TSource, TNext, (Func<TSource, bool> Predicate, Func<TSource, TNext> Selector)>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in (Func<TSource, bool> Predicate, Func<TSource, TNext> Selector) state)
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref builder, span, ref localCopy, state.Predicate, state.Selector);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TSourceDuplicate, TCurrent, TResult, TProcessStream>(in ReadOnlySpan<TSourceDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TSourceDuplicate, TSource>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TSource, TNext, TCurrent, TResult, TProcessStream, (Func<TSource, bool> Predicate, Func<TSource, TNext> Selector), Execute>(0, ref span, in processStream, (_predicate, _selector));
    }
}
