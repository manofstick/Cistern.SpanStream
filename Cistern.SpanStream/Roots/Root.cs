using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct Root<TInitial>
    : IStreamNode<TInitial, TInitial>
{
    int? IStreamNode<TInitial, TInitial>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Null { }

    struct Execute
        : StackAllocator.IAfterAllocation<TInitial, TInitial, Null>
    {
        TResult StackAllocator.IAfterAllocation<TInitial, TInitial, Null>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> builder, in ReadOnlySpan<TInitial> span, in TProcessStream stream, in Null selector)
        {
            var localCopy = stream;
            Iterator.Vanilla(ref builder, in span, ref localCopy);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TInitial, TInitial>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitial> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        return StackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Null, Execute>(stackAllocationCount, in span, in processStream, default);
    }
}
