using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<T>
    : IStreamNode<T, T>
{
    public readonly Func<T, bool> Predicate;

    public WhereRoot(Func<T, bool> predicate) => Predicate = predicate;

    int? IStreamNode<T, T>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : StackAllocator.IAfterAllocation<T, T, Func<T, bool>>
    {
        TResult StackAllocator.IAfterAllocation<T, T, Func<T, bool>>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> state, in ReadOnlySpan<T> span, in TProcessStream stream, in Func<T, bool> predicate)
        {
            var localCopy = stream;
            Iterator.Where(ref state, in span, ref localCopy, predicate);
            return localCopy.GetResult(ref state);
        }
    }

    TResult IStreamNode<T, T>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<T> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        return StackAllocator.Execute<T, T, TFinal, TResult, TProcessStream, Func<T, bool>, Execute>(stackAllocationCount, in span, in processStream, Predicate);
    }
}
