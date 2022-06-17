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
