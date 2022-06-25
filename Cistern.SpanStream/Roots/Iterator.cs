using Cistern.Utils;

namespace Cistern.SpanStream.Roots
{
    internal static class Iterator
    {
        public static void Forward<TInitial, TFinal, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, in s[i]))
                    break;
            }
        }

        public static void Reverse<TInitial, TFinal, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            var s = span;
            for (var i = s.Length-1; i >= 0; --i)
            {
                if (!stream.ProcessNext(ref state, in s[i]))
                    break;
            }
        }

        internal static void Where<TInitial, TFinal, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (predicate(s[i]))
                {
                    if (!stream.ProcessNext(ref state, in s[i]))
                        break;
                }
            }
        }

        internal static void SelectWhere<TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector, Func<TNext, bool> predicate)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                var next = selector(s[i]);
                if (predicate(next))
                {
                    if (!stream.ProcessNext(ref state, next))
                        break;
                }
            }
        }

        internal static void Select<TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, selector(s[i])))
                    break;
            }
        }

        internal static void WhereSelect<TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (predicate(s[i]))
                {
                    if (!stream.ProcessNext(ref state, selector(s[i])))
                        break;
                }
            }
        }
        internal static TFinal[] SelectToArray<TInitial, TFinal>(in ReadOnlySpan<TInitial> span, Func<TInitial, TFinal> selector)
        {
            if (span.Length == 0)
                return Array.Empty<TFinal>();

            var s = span;
            var result = new TFinal[s.Length];
            for (var i = 0; i < s.Length; ++i)
                result[i] = selector(s[i]);
            return result;
        }
    }
}
