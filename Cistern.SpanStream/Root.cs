namespace Cistern.SpanStream;

public readonly struct RootNode<TSource>
    : IStreamNode<TSource>
{
    TResult IStreamNode<TSource>.Execute<TDuplicateSource, TResult, TProcessStream>(in ReadOnlySpan<TDuplicateSource> span, in TProcessStream processStream)
    {
        System.Diagnostics.Debug.Assert(typeof(TSource) == typeof(TDuplicateSource));

        var localCopy = processStream;

        foreach (var item in span)
            localCopy.ProcessNext((TSource)(object)item!);

        return localCopy.GetResult();
    }
}
