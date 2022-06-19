using Cistern.Utils;

namespace Cistern.SpanStream;

public interface IProcessStream<TElement, TCurrent>
{
    public bool ProcessNext(ref Builder<TCurrent> builder, in TElement input);
}

public interface IProcessStream<TElement, TCurrent, TResult>
    : IProcessStream<TElement, TCurrent>
{
    TResult GetResult(ref Builder<TCurrent> builder);
}

public interface IStreamNode<TSource>
{
    int? TryGetSize(int sourceSize, out int upperBound);
    TResult Execute<TRoot, TCurrent, TResult, TNextInChain>(in ReadOnlySpan<TRoot> span, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TSource, TCurrent, TResult>;
}

internal interface IExecuteIterator<TSource, TNext, State>
{
    TResult Execute<TCurrent, TResult, TProcessStream>(ref Builder<TCurrent> builder, ref Span<TSource> span, in TProcessStream stream, in State state)
        where TProcessStream : struct, IProcessStream<TNext, TCurrent, TResult>;
}
