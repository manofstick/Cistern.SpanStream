using Cistern.Utils;

namespace Cistern.SpanStream;

public interface IProcessStream<TElement, TFinal>
{
    public bool ProcessNext(ref StreamState<TFinal> builder, in TElement input);
}

public interface IProcessStream<TInput, TFinal, TResult>
    : IProcessStream<TInput, TFinal>
{
    TResult GetResult(ref StreamState<TFinal> builder);
}

public interface IStreamNode<TInitial, TInput>
{
    int? TryGetSize(int sourceSize, out int upperBound);
    TResult Execute<TFinal, TResult, TNextInStream>(in TNextInStream fenum, in ReadOnlySpan<TInitial> span, int? stackAllocationCount)
        where TNextInStream : struct, IProcessStream<TInput, TFinal, TResult>;
}
