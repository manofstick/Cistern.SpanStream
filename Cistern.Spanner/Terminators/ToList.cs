using Cistern.Utils;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Terminators;

public struct ToList<T>
    : IProcessStream<T, T, List<T>>
{
    readonly List<T> _builder;

    public ToList(int? maybeKnownSize)
    {
        _builder = maybeKnownSize.HasValue ? new(maybeKnownSize.Value) : new ();
    }

    List<T> IProcessStream<T, T, List<T>>.GetResult(ref StreamState<T> state) => _builder;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input)
    {
        _builder.Add(input);
        return true;
    }

    public static List<T> Execute<TInitial, TNode, TContext>(in ReadOnlySpan<TInitial> span, ref TNode source)
        where TNode : struct, IStreamNode<TInitial, T>
    {
        var maybeSize = source.TryGetSize(span.Length, out var upperBound);

        if (upperBound == 0)
            return new();

        ToList<T> toList = new(maybeSize);
        return source.Execute<T, List<T>, ToList<T>, TContext>(toList, span, null);
    }
}
