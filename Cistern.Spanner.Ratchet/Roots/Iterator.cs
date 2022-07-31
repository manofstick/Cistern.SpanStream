using Cistern.Utils;

namespace Cistern.Spanner.Ratchet.Roots;

internal static class IteratorEx
{
    internal static void SelectWhere<TInitial, TFinal, TNext, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, TNext> selector, Func<TNext, bool> predicate)
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
    internal static void WhereSelect<TInitial, TFinal, TNext, TProcessStream, TContext>(ref StreamState<TFinal> state, in ReadOnlySpan<TInitial> span, ref TProcessStream stream, Func<TInitial, bool> predicate, Func<TInitial, TNext> selector)
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
    internal static TFinal[] SelectToArray<TInitial, TFinal, TContext>(in ReadOnlySpan<TInitial> span, Func<TInitial, TFinal> selector)
    {
        if (span.Length == 0)
            return Array.Empty<TFinal>();

        var s = span;
        var result = new TFinal[s.Length];
        for (var i = 0; i < s.Length; ++i)
            result[i] = selector(s[i]);
        return result;
    }

    internal static List<TCurrent> WhereToList<TCurrent, TContext>(in ReadOnlySpan<TCurrent> span, Func<TCurrent, bool> predicate)
    {
        var result = new List<TCurrent>();
        var s = span;
        for (var i = 0; i < s.Length; ++i)
        {
            if (predicate(s[i]))
                result.Add(s[i]);
        }
        return result;
    }

    internal static int WhereCount<TCurrent, TContext>(in ReadOnlySpan<TCurrent> span, Func<TCurrent, bool> predicate)
    {
        var count = 0;
        var s = span;
        for (var i = 0; i < s.Length; ++i)
        {
            if (predicate(s[i]))
                ++count;
        }
        return count;
    }
}
