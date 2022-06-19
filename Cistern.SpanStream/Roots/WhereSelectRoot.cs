using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereSelectRoot<TInitial, TNext>
    : IStreamNode<TNext>
{
    readonly Func<TInitial, bool> _predicate;
    readonly Func<TInitial, TNext> _selector;

    public WhereSelectRoot(Func<TInitial, bool> predicate, Func<TInitial, TNext> selector) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TNext>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TInitial, TNext, (Func<TInitial, bool> Predicate, Func<TInitial, TNext> Selector)>
    {
        TResult IExecuteIterator<TInitial, TNext, (Func<TInitial, bool> Predicate, Func<TInitial, TNext> Selector)>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TInitial> span, in TProcessStream stream, in (Func<TInitial, bool> Predicate, Func<TInitial, TNext> Selector) state)
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref builder, span, ref localCopy, state.Predicate, state.Selector);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TNext>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInitial>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInitial, TNext, TFinal, TResult, TProcessStream, (Func<TInitial, bool> Predicate, Func<TInitial, TNext> Selector), Execute>(0, ref span, in processStream, (_predicate, _selector));
    }
}
