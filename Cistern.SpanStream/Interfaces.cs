using Cistern.Utils;

namespace Cistern.SpanStream;

public interface IProcessStream<TTerminatorState, TElement, TFinal>
{
    public bool ProcessNext(ref StreamState<TFinal, TTerminatorState> builder, in TElement input);
}

public interface IProcessStream<TTerminatorState, TInput, TFinal, TResult>
    : IProcessStream<TTerminatorState, TInput, TFinal>
{
    TResult GetResult(ref StreamState<TFinal, TTerminatorState> builder);
}

public interface IStreamNode<TInput>
{
    int? TryGetSize(int sourceSize, out int upperBound);
    TResult Execute<TTerminatorState, TInitialDuplicate, TFinal, TResult, TNextInChain>(in ReadOnlySpan<TInitialDuplicate> span, int? stackAllocationCount, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TTerminatorState, TInput, TFinal, TResult>;
}
