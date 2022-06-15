using System.Runtime.CompilerServices;

namespace Cistern.SpanStream;

public readonly struct RootNode<TSource>
    : INode<TSource>
{
    public TResult CreateViaPush<TRoot, TResult, TPushEnumerator>(in ReadOnlySpan<TRoot> span, in TPushEnumerator fenum)
        where TPushEnumerator : struct, IPushEnumerator<TSource>
    {
        var x = fenum;

        foreach (var item in span)
            x.ProcessNext((TSource)(object)item!);

        return x.GetResult<TResult>();
    }
}
