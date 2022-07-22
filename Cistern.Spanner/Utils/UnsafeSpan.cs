using System.Runtime.CompilerServices;

namespace Cistern.Spanner.Utils
{
    internal unsafe readonly struct UnsafeSpan<T>
    {
        readonly byte* _ptr;
        readonly int _sizeOfT;
        readonly int _length;

        public UnsafeSpan(void* ptr,int sizeOfT, int length)
        {
            _ptr = (byte*)ptr;
            _sizeOfT = sizeOfT;
            _length = length;
        }

        public static UnsafeSpan<T> FromStackAllocatedSpan(in Span<T> span) =>
            new (Unsafe.AsPointer(ref span.GetPinnableReference()), Unsafe.SizeOf<T>(), span.Length);

        public int Length => _length;

        public ref T this[int i] => ref Unsafe.AsRef<T>(_ptr + i * _sizeOfT);
    }
}
