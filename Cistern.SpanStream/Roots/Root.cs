using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct Root<TInitial>
    : IStreamNode<TInitial>
{
    int? IStreamNode<TInitial>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Null { }

    struct Execute
        : StackAllocator.IAfterAllocation<TInitial, TInitial, Null>
    {
        TResult StackAllocator.IAfterAllocation<TInitial, TInitial, Null>.Execute<TCurrent, TResult, TProcessStream>(ref StreamState<TCurrent> builder, ref Span<TInitial> span, in TProcessStream stream, in Null selector)
        {
            var localCopy = stream;
            Iterator.Vanilla(ref builder, span, ref localCopy);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TInitial>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, int? stackAllocationCount, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInitial>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Null, Execute>(stackAllocationCount, ref span, in processStream, default);
    }
}
