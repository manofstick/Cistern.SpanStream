using Cistern.Spanner;
using System.Runtime.InteropServices;

namespace Cistern.Utils;

internal static class LargeStackAllocator
{
    internal interface IAfterAllocation<TInitial, TNext, TState>
    {
        TResult Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream stream, in ReadOnlySpan<TInitial> span, ref StreamState<TFinal> builder, in TState state)
            where TProcessStream : struct, IProcessStream<TNext, TFinal, TResult>;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BufferStorage<T>
    {
        public T _01;
        public T _02;
        public T _03;
        public T _04;
        public T _05;
        public T _06;
        public T _07;
        public T _08;
        public T _09;
        public T _10;
        public T _11;
        public T _12;
        public T _13;
        public T _14;
        public T _15;
        public T _16;
        public T _17;
        public T _18;
        public T _19;
        public T _20;
        public T _21;
        public T _22;
        public T _23;
        public T _24;
        public T _25;
        public T _26;
        public T _27;
        public T _28;
        public T _29;
        public T _30;

        public const int NumberOfElements = 30;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SequentialDataPair<T>
    {
        public T Item1;
        public T Item2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MemoryChunk<T, TChunk>
        where TChunk : struct
    {
        public T Head;
        public TChunk Tail;
    }

    static TResult AllocateAndExecute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, TCurrentChunk, TContext>(in ReadOnlySpan<TInitial>  span, in TProcessStream stream, in TArgs args, int requiredSize, int currentSize)
        where TProcessStream : struct, IProcessStream<TNext, TCurrent, TResult>
        where TExecution : struct, IAfterAllocation<TInitial, TNext, TArgs>
        where TCurrentChunk : struct
    {
        MemoryChunk<TCurrent, TCurrentChunk> chunkOfStackSpace = default;
        BufferStorage<TCurrent[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace.Head, currentSize);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, BufferStorage<TCurrent>.NumberOfElements);

        StreamState<TCurrent> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return default(TExecution).Execute<TCurrent, TResult, TProcessStream, TContext>(in stream, in span, ref state, in args);
    }

    static TResult BuildStackObjectAndExecute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, TCurrentChunk, TContext>(in ReadOnlySpan<TInitial>  span, in TProcessStream stream, in TArgs args, int requiredSize, int currentSize)
        where TProcessStream : struct, IProcessStream<TNext, TCurrent, TResult>
        where TExecution : struct, IAfterAllocation<TInitial, TNext, TArgs>
        where TCurrentChunk :struct
    {
        var nextSizeUp = ((currentSize - 1) * 2) + 1;

        if (currentSize < requiredSize)
            return BuildStackObjectAndExecute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, SequentialDataPair<TCurrentChunk>, TContext>(in span, in stream, in args, requiredSize, nextSizeUp);
        else
            return AllocateAndExecute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, TCurrentChunk, TContext>(in span, in stream, in args, requiredSize, currentSize);
    }

    public static TResult Execute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, TContext>(int stackAllocationCount, in ReadOnlySpan<TInitial> span, in TProcessStream stream, in TArgs args)
        where TProcessStream : struct, IProcessStream<TNext, TCurrent, TResult>
        where TExecution : struct, IAfterAllocation<TInitial, TNext, TArgs>
    {
        return BuildStackObjectAndExecute<TInitial, TNext, TCurrent, TResult, TProcessStream, TArgs, TExecution, SequentialDataPair<BufferStorage<TCurrent>>, TContext>(in span, in stream, in args, stackAllocationCount, (BufferStorage<TCurrent>.NumberOfElements * 2) + 1/*Head*/);
    }
}

public ref struct StreamState<T>
{
    internal /*readonly*/ Span<T[]?> Buffers;
    internal /*readonly*/ Span<T> Root;

    internal Span<T> Current;

    public StreamState(Span<T[]?> bufferStore, Span<T> initalBuffer)
    {
        Root = initalBuffer;
        Buffers = bufferStore;

        Current = initalBuffer;
    }
}
