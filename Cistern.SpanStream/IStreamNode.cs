namespace Cistern.SpanStream;

public interface IStreamNode<TSource>
{
    TResult Execute<TRoot, TResult, TNextInChain>(in ReadOnlySpan<TRoot> span, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TSource, TResult>;
}
