namespace Cistern.Utils
{
    internal static class ThrowHelper
    {
        public static void SequenceContainsNoElements() => throw new InvalidOperationException("Sequence contains no elements");
    }
}
