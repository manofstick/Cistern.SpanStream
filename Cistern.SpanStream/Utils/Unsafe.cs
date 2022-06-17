using System.Runtime.InteropServices;

namespace Cistern.SpanStream.Utils;

internal unsafe static class Unsafe
{
    public static Span<TOut> SpanCast<TIn, TOut>(in ReadOnlySpan<TIn> span)
    {
        System.Diagnostics.Debug.Assert(typeof(TIn) == typeof(TOut));

        return
            MemoryMarshal.CreateSpan(
                ref System.Runtime.CompilerServices.Unsafe.AsRef<TOut>(
                        System.Runtime.CompilerServices.Unsafe.AsPointer(
                            ref MemoryMarshal.GetReference(span)
                        )
                    ),
                span.Length);
    }
}
