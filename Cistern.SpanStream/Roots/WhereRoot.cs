using Cistern.SpanStream.Utils;
using Cistern.Utils;

namespace Cistern.SpanStream.Roots;

public readonly struct WhereRoot<TInitial>
    : IStreamNode<TInitial>
{
    public readonly Func<TInitial, bool> Predicate;

    public WhereRoot(Func<TInitial, bool> predicate) => Predicate = predicate;

    int? IStreamNode<TInitial>.TryGetSize(int sourceSize, out int upperBound)
    {
        upperBound = sourceSize;
        return null;
    }

    struct Execute
        : IExecuteIterator<TInitial, TInitial, Func<TInitial, bool>>
    {
        TResult IExecuteIterator<TInitial, TInitial, Func<TInitial, bool>>.Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TInitial> span, in TProcessStream stream, in Func<TInitial, bool> predicate)
        {
            var localCopy = stream;
            Iterator.Where(ref builder, span, ref localCopy, predicate);
            return localCopy.GetResult(ref builder);
        }
    }

    TResult IStreamNode<TInitial>.Execute<TInitialDuplicate, TFinal, TResult, TProcessStream>(in ReadOnlySpan<TInitialDuplicate> spanAsSourceDuplicate, in TProcessStream processStream)
    {
        var span = Unsafe.SpanCast<TInitialDuplicate, TInitial>(spanAsSourceDuplicate);

        return StackAllocator.Execute<TInitial, TInitial, TFinal, TResult, TProcessStream, Func<TInitial, bool>, Execute>(0, ref span, in processStream, Predicate);
    }
}
