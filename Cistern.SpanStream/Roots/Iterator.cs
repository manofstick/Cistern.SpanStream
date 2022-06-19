using Cistern.Utils;

namespace Cistern.SpanStream.Roots
{
    internal static class Iterator
    {
        public static void Vanilla<TTerminatorState, TInitial, TFinal, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, Span<TInitial> span, ref TProcessStream stream)
            where TProcessStream : struct, IProcessStream<TTerminatorState, TInitial, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, in span[i]))
                    break;
            }
        }

        internal static void Where<TTerminatorState, TInitial, TFinal, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate)
            where TProcessStream : struct, IProcessStream<TTerminatorState, TInitial, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (predicate(span[i]))
                {
                    if (!stream.ProcessNext(ref state, in span[i]))
                        break;
                }
            }
        }

        internal static void SelectWhere<TTerminatorState, TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector, Func<TNext, bool> predicate)
            where TProcessStream : struct, IProcessStream<TTerminatorState, TNext, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                var next = selector(span[i]);
                if (predicate(next))
                {
                    if (!stream.ProcessNext(ref state, next))
                        break;
                }
            }
        }

        internal static void Select<TTerminatorState, TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TTerminatorState, TNext, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!stream.ProcessNext(ref state, selector(span[i])))
                    break;
            }
        }

        internal static void WhereSelect<TTerminatorState, TInitial, TFinal, TNext, TProcessStream>(ref StreamState<TFinal, TTerminatorState> state, Span<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate, Func<TInitial, TNext> selector)
            where TProcessStream : struct, IProcessStream<TTerminatorState, TNext, TFinal>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (predicate(span[i]))
                {
                    if (!stream.ProcessNext(ref state, selector(span[i])))
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
