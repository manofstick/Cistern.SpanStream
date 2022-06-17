namespace Cistern.SpanStream;

public interface IProcessStream<TElement>
{
    public bool ProcessNext(in TElement input);
}

public interface IProcessStream<TElement, TResult>
    : IProcessStream<TElement>
{
    TResult GetResult();
}

public interface IStreamNode<TSource>
{
    TResult Execute<TRoot, TResult, TNextInChain>(in ReadOnlySpan<TRoot> span, in TNextInChain fenum)
        where TNextInChain : struct, IProcessStream<TSource, TResult>;
}
