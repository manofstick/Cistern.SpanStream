using Cistern.Utils;

namespace Cistern.SpanStream;

public interface IProcessStream<TElement, TFinal>
{
    public bool ProcessNext(ref Builder<TFinal> builder, in TElement input);
}

public interface IProcessStream<TInput, TFinal, TResult>
    : IProcessStream<TInput, TFinal>
{
    TResult GetResult(ref Builder<TFinal> builder);
}

public interface IStreamNode<TInput>
{
    int? TryGetSize(int sourceSize, out int upperBound);
    TResult Execute<TInitialDuplicate, TFinal, TResult, TNextInChain>(in ReadOnlySpan<TInitialDuplicate> span, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TInput, TFinal, TResult>;
}

internal interface IExecuteIterator<TInitial, TNext, TState>
{
    TResult Execute<TFinal, TResult, TProcessStream>(ref Builder<TFinal> builder, ref Span<TInitial> span, in TProcessStream stream, in TState state)
        where TProcessStream : struct, IProcessStream<TNext, TFinal, TResult>;
}
