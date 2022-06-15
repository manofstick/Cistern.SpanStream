namespace Cistern.SpanStream;

public interface IPushEnumerator<TElement>
{
    public bool ProcessNext(TElement input);
    TResult GetResult<TResult>();
}
