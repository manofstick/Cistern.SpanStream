using Cistern.Utils;
using System.Runtime.InteropServices;

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
        : LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Null>
    {
        public static TResult Invoke<TFinal, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> state)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
        {
            var localCopy = stream;
            Iterator.Vanilla(ref state, in span, ref localCopy);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Null>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> state, in Null selector)
            => Invoke<TFinal, TResult, TProcessStream>(in stream, in span, ref state);
    }

    TResult IStreamNode<TInitial, TInitial>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else
            return LargeStackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Null, Execute>(stackAllocationCount.Value, in span, in processStream, default);
    }

    private TResult NoStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state);
    }

    private static TResult ExecuteSmallStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state);
    }
}
