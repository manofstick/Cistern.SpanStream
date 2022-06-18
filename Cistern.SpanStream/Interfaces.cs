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
    TResult Execute<TRoot, TCurrent, TResult, TNextInChain>(in ReadOnlySpan<TRoot> span, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TSource, TCurrent, TResult>;
}
