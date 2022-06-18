using Cistern.Utils;

namespace Cistern.SpanStream.Roots
{
    internal static class Iterator
    {
        public static void Vanilla<TSource, TCurrent, TProcessStream>(ref Builder<TCurrent> builder, Span<TSource> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TSource, TCurrent>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref builder, in span[i]))
                    break;
            }
        }

        internal static void Where<TSource, TCurrent, TProcessStream>(ref Builder<TCurrent> builder, Span<TSource> span, ref TProcessStream stream, Func<TSource, bool> predicate)
            where TProcessStream : struct, IProcessStream<TSource, TCurrent>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (predicate(span[i]))
                {
                    if (!stream.ProcessNext(ref builder, in span[i]))
                        break;
                }
            }
        }

        internal static void SelectWhere<TSource, TCurrent, TNext, TProcessStream>(ref Builder<TCurrent> builder, Span<TSource> span, ref TProcessStream stream, Func<TSource, TNext> selector, Func<TNext, bool> predicate)
            where TProcessStream : struct, IProcessStream<TNext, TCurrent>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                var next = selector(span[i]);
                if (predicate(next))
                {
                    if (!stream.ProcessNext(ref builder, next))
                        break;
                }
            }
        }

        internal static void Select<TSource, TCurrent, TNext, TProcessStream>(ref Builder<TCurrent> builder, Span<TSource> span, ref TProcessStream stream, Func<TSource, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TCurrent>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref builder, selector(span[i])))
                    break;
            }
        }

        internal static void WhereSelect<TSource, TCurrent, TNext, TProcessStream>(ref Builder<TCurrent> builder, Span<TSource> span, ref TProcessStream stream, Func<TSource, bool> predicate, Func<TSource, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TCurrent>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (predicate(span[i]))
                {
                    if (!stream.ProcessNext(ref builder, selector(span[i])))
                        break;
                }
            }
        }
    }
}
