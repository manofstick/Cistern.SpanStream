﻿using Cistern.Utils;
using System.Runtime.InteropServices;

namespace Cistern.SpanStream.Roots;


public readonly struct SelectRoot<TInput, TOutput>
    : IStreamNode<TInput, TOutput>
{
    public Func<TInput, TOutput> Selector { get; }

    public SelectRoot(Func<TInput, TOutput> selector) =>
        Selector = selector;

    int? IStreamNode<TInput, TOutput>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return sourceSize;
    }

    struct Execute
        : LargeStackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>
    {
        public static TResult Invoke<TFinal, TResult, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInput> span, in TProcessStream stream, in Func<TInput, TOutput> selector)
            where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
        {
            var localCopy = stream;
            Iterator.Select(ref state, in span, ref localCopy, selector);
            return localCopy.GetResult(ref state);
        }

        TResult LargeStackAllocator.IAfterAllocation<TInput, TOutput, Func<TInput, TOutput>>.Execute<TFinal, TResult, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInput> span, in TProcessStream stream, in Func<TInput, TOutput> selector) =>
            Invoke<TFinal, TResult, TProcessStream>(ref state, in span, in stream, selector);
    }

    TResult IStreamNode<TInput, TOutput>.Execute<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, int? stackAllocationCount, in TProcessStream processStream)
    {
        if (!stackAllocationCount.HasValue || stackAllocationCount <= 0)
            return NoStack<TFinal, TResult, TProcessStream>(in span, in processStream);
        else if (stackAllocationCount <= 30)
            return ExecuteSmallStack<TFinal, TResult, TProcessStream>(in span, in processStream);
        else
            return LargeStackAllocator.Execute<TInput, TOutput, TFinal, TResult, TProcessStream, Func<TInput, TOutput>, Execute>(stackAllocationCount.Value, in span, in processStream, Selector);
    }

    private TResult NoStack<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, in TProcessStream processStream)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        StreamState<TFinal> state = default;
        return Execute.Invoke<TFinal, TResult, TProcessStream>(ref state, in span, in processStream, Selector);
    }

    private TResult ExecuteSmallStack<TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInput> span, in TProcessStream processStream)
        where TProcessStream : struct, IProcessStream<TOutput, TFinal, TResult>
    {
        LargeStackAllocator.BufferStorage<TFinal> chunkOfStackSpace = default;
        LargeStackAllocator.BufferStorage<TFinal[]?> bufferStorage = default;

        var spanOfTCurrent = MemoryMarshal.CreateSpan(ref chunkOfStackSpace._01, LargeStackAllocator.BufferStorage<TFinal>.NumberOfElements);
        var spanOfTCurrentArray = MemoryMarshal.CreateSpan(ref bufferStorage._01, LargeStackAllocator.BufferStorage<TFinal[]?>.NumberOfElements);

        StreamState<TFinal> state = new(spanOfTCurrentArray, spanOfTCurrent);
        return Execute.Invoke<TFinal, TResult, TProcessStream>(ref state, in span, in processStream, Selector);
    }
}
