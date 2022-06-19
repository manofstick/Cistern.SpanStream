using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct SelectWhereRoot<TInput, TOutput>
    : IStreamNode<TOutput>
{
    readonly Func<TInput, TOutput> _selector;
    readonly Func<TOutput, bool> _predicate;

    public SelectWhereRoot(Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (_predicate, _selector) = (predicate, selector);

    int? IStreamNode<TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>
    {
        TResult IExecuteIterator<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> state, ref Span<TInput> span, in TProcessStream stream, in (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate) args)
        {
            var localCopy = stream;
            Iterator.SelectWhere(ref state, span, ref localCopy, args.Selector, args.Predicate);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<TOutput>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInput>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate), Execute>(0, ref span, in processStream, (_selector, _predicate));
    }
}
