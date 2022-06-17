namespace Cistern.SpanStream.Roots
{
    internal static class Iterator
    {
        public static void Run<TSource, TProcessStream>(Span<TSource> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TSource>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(in span[i]))
                    break;
            }
        }

        internal static void Run<TSource, TProcessStream>(Span<TSource> span, ref TProcessStream stream, Func<TSource, bool> predicate)
            where TProcessStream : struct, IProcessStream<TSource>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (predicate(span[i]))
                {
                    if (!stream.ProcessNext(in span[i]))
                        break;
                }
            }
        }
    }
}
