namespace Cistern.Spanner.Utils
{
    internal class UninitializedArrayStandIn<T>
    {
        static readonly internal T[] Instance = new T[0]; // not Array<T>.Empty!!
    }

    internal class UninitializedArrayStandIn
    {
        internal static bool IsArrayUninitialized<T>(T[] array) => ReferenceEquals(array, UninitializedArrayStandIn<T>.Instance);
    }
}
