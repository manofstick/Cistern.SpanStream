using Cistern.Utils;

namespace Cistern.Spanner.Roots
{
    internal static class Iterator
    {
        public static void Forward<TInitial, TFinal, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, in s[i]))
                    break;
            }
        }

        public static void Reverse<TInitial, TFinal, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TInitial, TFinal>
        {
            var s = span;
            for (var i = s.Length-1; i >= 0; --i)
            {
                if (!stream.ProcessNext(ref state, in s[i]))
                    break;
            }
        }

        internal static void Where<TInitial, TFinal, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate)
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

        internal static void Select<TInitial, TFinal, TNext, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TNext, TFinal>
        {
            var s = span;
            for (var i = 0; i < s.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, selector(s[i])))
                    break;
            }
        }
    }
}
