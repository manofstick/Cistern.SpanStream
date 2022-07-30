using Cistern.Utils;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Roots;

public /*readonly*/ struct Root<TInitial>
    : IStreamNode<TInitial, TInitial>
{
    internal static Root<TInitial> Instance = new ();

    int? IStreamNode<TInitial, TInitial>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Null { }

    struct Executer
        : LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Null>
    {
        public static TResult Invoke<TFinal, TResult, TProcessStream, TContext>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> state)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
        {
            var localCopy = stream;
            Iterator.Forward<TInitial, TFinal, TProcessStream, TContext>(ref state, in span, ref localCopy);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInitial, TInitial, Null>.Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> state, in Null selector)
            => Invoke<TFinal, TResult, TProcessStream, TContext>(in stream, in span, ref state);
    }

    public static TResult Execute<TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount = null)
        where TProcessStream : struct, IProcessStream<TInitial, TInitial, TResult>
    {
        return Invoke(ref Root<TInitial>.Instance, in processStream, in span, stackAllocationCount);

        static TResult Invoke<TRootInitial>(ref TRootInitial root, in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
            where TRootInitial : IStreamNode<TInitial, TInitial> =>
            root.Execute<TInitial, TResult, TProcessStream, TContext>(in processStream, in span, stackAllocationCount);
    }

    public TResult Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream, TContext>(in processStream, in span);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream, TContext>(in processStream, in span);
        else
            return LargeStackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Null, Executer, TContext>(stackAllocationCount.Value, in span, in processStream, default);
    }

    private TResult NoStack<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Executer.Invoke<TFinal, TResult, TProcessStream, TContext>(in processStream, in span, ref state);
    }

    private static TResult ExecuteSmallStack<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span)
        where TProcessStream : struct, IProcessStream<TInitial, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Executer.Invoke<TFinal, TResult, TProcessStream, TContext>(in processStream, in span, ref state);
    }

    public bool TryGetNext(ref EnumeratorState<TInitial> state, out TInitial current)
    {
        if (state.Index < state.Span.Length)
        {
            current = state.Span[state.Index++];
            return true;
        }
        current = default!;
        return false;
    }
}
