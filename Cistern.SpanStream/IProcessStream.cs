namespace Cistern.SpanStream;

public interface IProcessStream<TElement, TResult>
{
    public bool ProcessNext(TElement input);
    TResult GetResult();
}
