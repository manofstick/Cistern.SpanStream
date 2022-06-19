using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectWhereRoot<TInitial, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TInitial, TNext> _selector;
    readonly Func<TNext, bool> _predicate;

    public SelectWhereRoot(Func<TInitial, TNext> selector, Func<TNext, bool> predicate) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TInitial, TNext, (Func<TInitial, TNext> Selector, Func<TNext, bool> Predicate)>
    {
        TResult IExecuteIterator<TInitial, TNext, (Func<TInitial, TNext> Selector, Func<TNext, bool> Predicate)>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TInitial> span, in TProcessStream stream, in (Func<TInitial, TNext> Selector, Func<TNext, bool> Predicate) state)
        {
            var localCopy = stream;
            Iterator.SelectWhere(ref builder, span, ref localCopy, state.Selector, state.Predicate);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInitial>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInitial, TNext, TFinal, TResult, TProcessStream, (Func<TInitial, TNext> Selector, Func<TNext, bool> Predicate), Execute>(0, ref span, in processStream, (_selector, _predicate));
    }
}
