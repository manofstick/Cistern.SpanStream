using Cistern.Utils;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Roots;

public /*readonly*/ struct WhereRoot<TInitial>
    : IStreamNode<TInitial, TInitial>
{
    public Func<TInitial, bool> Predicate { get; }

    public WhereRoot(Func<TInitial, bool> predicate) => Predicate = predicate;

    int? IStreamNode<TInitial, TInitial>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Func<TInitial, bool>>
    {
        public static TResult Invoke<TCurrent, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TCurrent> state, Func<TInitial, bool> predicate)
            where TProcessStream : struct, IProcessStream<TInitial, TCurrent, TResult>
        {
            var localCopy = stream;
            Iterator.Where(ref state, in span, ref localCopy, predicate);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Func<TInitial, bool>>.Execute<TCurrent, TResult, TProcessStream>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TCurrent> state, in Func<TInitial, bool> predicate) =>
            Invoke<TCurrent, TResult, TProcessStream>(in stream, in span, ref state, predicate);
    }

    TResult IStreamNode<TInitial, TInitial>.Execute<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream>(in processStream, in span);
        else
            return LargeStackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Func<TInitial, bool>, Execute>(stackAllocationCount.Value, in span, in processStream, Predicate);
    }

    private TResult NoStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Predicate);
    }

    private TResult ExecuteSmallStack<TFinal, TResult, TProcessStream>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Execute.Invoke<TFinal, TResult, TProcessStream>(in processStream, in span, ref state, Predicate);
    }

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TInitial current)
    {
        var idx = state.Index;
        var span = state.Span;
        while (idx < span.Length)
        {
            var c = span[idx++];
            if (Predicate(c))
            {
                state.Index = idx;
                current = c;
                return true;
            }
        }
        state.Index = idx;
        current = default!;
        return false;
    }
}
