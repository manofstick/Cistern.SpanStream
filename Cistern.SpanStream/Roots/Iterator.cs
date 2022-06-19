using Cistern.Utils;

namespace Cistern.SpanStream.Roots
{
    internal static class Iterator
    {
        public static void Vanilla<TInitial, TFinal, TProcessStream>(ref Builder<TFinal> builder, Span<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref builder, in span[i]))
                    break;
            }
        }

        internal static void Where<TInitial, TFinal, TProcessStream>(ref Builder<TFinal> builder, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
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

        internal static void SelectWhere<TInitial, TFinal, TNext, TProcessStream>(ref Builder<TFinal> builder, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector, Func<TNext, bool> predicate)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
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

        internal static void Select<TInitial, TFinal, TNext, TProcessStream>(ref Builder<TFinal> builder, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref builder, selector(span[i])))
                    break;
            }
        }

        internal static void WhereSelect<TInitial, TFinal, TNext, TProcessStream>(ref Builder<TFinal> builder, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
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
        internal static TFinal[] SelectToArray<TInitial, TFinal>(ReadOnlySpan<TInitial> span, Func<TInitial, TFinal> selector)
        {
            var result = new TFinal[span.Length];
            for (var i = 0; i < span.Length; ++i)
                result[i] = selector(span[i]);
            return result;
        }
    }
}
