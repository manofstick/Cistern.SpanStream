using Cistern.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cistern.Spanner.Ratchet.Transforms;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AppendItem<T>
{
    public T Item;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AppendItems<TPreviousElementsBuffer, T>
    where TPreviousElementsBuffer : struct
{
    public TPreviousElementsBuffer PreviousItems;
    public T CurrentItem;
}

public /*readonly*/ struct AppendN<TInitial, TInput, TPreviousElementsBuffer, TPriorNode>
    : IStreamNode<TInitial, TInput>
    where TPriorNode : struct, IStreamNode<TInitial, TInput>
    where TPreviousElementsBuffer : struct
{
    internal /*readonly*/ TPriorNode Node;

    private AppendItems<TPreviousElementsBuffer, TInput> _items;
    public AppendItems<TPreviousElementsBuffer, TInput> Items => _items;
    internal int Count { get; }

    private int _appended;

    public AppendN(ref TPriorNode nodeT, TPreviousElementsBuffer previousItems, TInput currentItem, int count) =>
        (Node, _items, Count, _appended) = (nodeT, new () { PreviousItems = previousItems, CurrentItem = currentItem }, count, 0);

    int? IStreamNode<TInitial, TInput>.TryGetSize(int sourceSize, out int upperBound)
    {
        var maybeSize = Node.TryGetSize(sourceSize, out upperBound);
        upperBound += Count;
        return maybeSize + Count;
    }

    TResult IStreamNode<TInitial, TInput>.Execute<TFinal, TResult, TProcessStream, TContext>(in TProcessStream processStream, in ReadOnlySpan<TInitial> span, int? stackAllocationCount) =>
        Node.Execute<TFinal, TResult, AppendNStream<TInput, TFinal, TResult, TPreviousElementsBuffer, TProcessStream>, TContext>(new(in processStream, _items, Count), in span, stackAllocationCount);

    unsafe public bool TryGetNext(ref EnumeratorState<TInitial> state, out TInput current)
    {
        if (Node.TryGetNext(ref state, out current))
            return true;

        if (_appended < Count)
        {
            var spanOfTCurrent = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<TInput>(Unsafe.AsPointer(ref _items.PreviousItems)), Count);

            current = spanOfTCurrent[_appended];
            ++_appended;
            return true;
        }

        current = default!;
        return false;
    }
}

struct AppendNStream<TInput, TFinal, TResult, TPreviousElementsBuffer, TProcessStream>
    : IProcessStream<TInput, TFinal, TResult>
    where TProcessStream : struct, IProcessStream<TInput, TFinal, TResult>
    where TPreviousElementsBuffer : struct
{
    /* can't be readonly */
    TProcessStream _next;
    AppendItems<TPreviousElementsBuffer, TInput> _items;
    int _count;
    bool _continue;

    public AppendNStream(in TProcessStream nextProcessStream, AppendItems<TPreviousElementsBuffer, TInput> items, int count) =>
        (_next, _items, _count, _continue) = (nextProcessStream, items, count, true);

    unsafe TResult IProcessStream<TInput, TFinal, TResult>.GetResult(ref StreamState<TFinal> state)
    {
        if (_continue)
        {
            var spanOfTCurrent = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<TInput>(Unsafe.AsPointer(ref _items.PreviousItems)), _count);
            foreach (var item in spanOfTCurrent)
            {
                if (!_next.ProcessNext(ref state, item))
                    break;
            }
        }
        return _next.GetResult(ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TInput, TFinal>.ProcessNext(ref StreamState<TFinal> state, in TInput input) =>
        _continue = _next.ProcessNext(ref state, input);
}
