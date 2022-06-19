using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereSelectRoot<TInput, TOutput>
    : IStreamNode<TOutput>
{
    readonly Func<TInput, bool> _predicate;
    readonly Func<TInput, TOutput> _selector;

    public WhereSelectRoot(Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>
    {
        TResult IExecuteIterator<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TInput> span, in TProcessStream stream, in (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector) state)
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref builder, span, ref localCopy, state.Predicate, state.Selector);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TOutput>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInput>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector), Execute>(0, ref span, in processStream, (_predicate, _selector));
    }
}
