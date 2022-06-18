using Cistern.Utils;
using System.Runtime.CompilerServices;

namespace Cistern.SpanStream.Terminators;

public struct ToArray<TSource>
    : IProcessStream<TSource, TSource, TSource[]>
{
    public ToArray() { }

    TSource[] IProcessStream<TSource, TSource, TSource[]>.GetResult(ref Builder<TSource> builder) => builder.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IProcessStream<TSource, TSource>.ProcessNext(ref Builder<TSource> builder, in TSource input)
    {
        builder.Add(input);
        return true;
    }
}
