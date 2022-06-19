using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<T>
    : IStreamNode<T>
{
    public readonly Func<T, bool> Predicate;

    public WhereRoot(Func<T, bool> predicate) => Predicate = predicate;

    int? IStreamNode<T>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<T, T, Func<T, bool>>
    {
        TResult StackAllocator.IAfterAllocation<T, T, Func<T, bool>>.Execute<TTerminatorState, TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent, TTerminatorState> state, ref Span<T> span, in TProcessStream stream, in Func<T, bool> predicate)
        {
            var localCopy = stream;
            Iterator.Where(ref state, span, ref localCopy, predicate);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<T>.Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, int? stackAllocationCount, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, T>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TTerminatorState, T, T, TFinal, TResult, TProcessStream, Func<T, bool>, Execute>(stackAllocationCount, ref span, in processStream, Predicate);
    }
}
