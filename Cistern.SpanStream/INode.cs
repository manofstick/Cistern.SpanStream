namespace Cistern.SpanStream;

public interface INode<T>
{
    TResult CreateViaPush<TRoot, TResult, TPushEnumerator>(in ReadOnlySpan<TRoot> span, in TPushEnumerator fenum)
        where TPushEnumerator : struct, IPushEnumerator<T>;
}
