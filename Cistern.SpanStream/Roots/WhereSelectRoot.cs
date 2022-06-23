using Cistern.Utils;
using System.Runtime.InteropServices;

namespace Cistern.SpanStream.Roots;

public /*readonly*/ struct WhereSelectRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    public Func<TInput, bool> Predicate { get; }
    public Func<TInput, TOutput> Selector { get; }

    public WhereSelectRoot(Func<TInput, bool> predicate, Func<TInput, TOutput> selector) =>
        (Predicate, Selector) = (predicate, selector);

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : LargeStackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>
    {
        public static TResult Invoke<TFinal, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInput> span, ref StreamState<TFinal> state, Func<TInput, bool> predicate, Func<TInput, TOutput> selector)
            where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
        {
            var localCopy = stream;
            Iterator.WhereSelect(ref state, in span, ref localCopy, predicate, selector);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector)>.Execute<TCurrent, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInput> span, ref StreamState<TCurrent> state, in (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector) args) =>
            Invoke<TCurrent, TResult, TProcessStream>(in stream, in span, ref state, args.Predicate, args.Selector);
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span, int? stackAllocationCount)
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else
            return LargeStackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, bool> Predicate, Func<TInput, TOutput> Selector), Execute>(stackAllocationCount.Value, in span, in processStream, (Predicate, Selector));
    }

    private TResult NoStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Predicate, Selector);
    }

    private TResult ExecuteSmallStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Predicate, Selector);
    }
}
