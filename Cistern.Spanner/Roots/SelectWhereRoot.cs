using Cistern.Utils;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Roots;

public /*readonly*/ struct SelectWhereRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    public Func<TInput, TOutput> Selector { get; }
    public Func<TOutput, bool> Predicate { get; }

    public SelectWhereRoot(Func<TInput, TOutput> selector, Func<TOutput, bool> predicate) =>
        (Predicate, Selector) = (predicate, selector);

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : LargeStackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>
    {
        public static TResult Invoke<TCurrent, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInput> span, ref StreamState<TCurrent> state, Func<TInput, TOutput> selector, Func<TOutput, bool> predicate)
            where TProcessStream : struct, IProcessStream<TOutput, TCurrent, TResult>
        {
            var localCopy = stream;
            Iterator.SelectWhere(ref state, in span, ref localCopy, selector, predicate);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInput, TOutput, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate)>.Execute<TCurrent, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInput> span, ref StreamState<TCurrent> state, in (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate) args) =>
            Invoke<TCurrent, TResult, TProcessStream>(in stream, in span, ref state, args.Selector, args.Predicate);
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span, int? stackAllocationCount)
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else
            return LargeStackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, (Func<TInput, TOutput> Selector, Func<TOutput, bool> Predicate), Execute>(stackAllocationCount.Value, in span, in processStream, (Selector, Predicate));
    }

    private TResult NoStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Selector, Predicate);
    }

    private TResult ExecuteSmallStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInput> span)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Selector, Predicate);
    }

    public bool TryGetNext(ref EnumeratorState<TInput> state, out TOutput current)
    {
        while (state.Index < state.Span.Length)
        {
            current = Selector(state.Span[state.Index++]);
            if (Predicate(current))
                return true;
        }
        current = default!;
        return false;
    }
}
