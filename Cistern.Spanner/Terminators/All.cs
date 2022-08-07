using Cistern.Utils;

namespace Cistern.Spanner.Terminators;

public struct All<T>
    : IProcessStream<T, T, bool>
{
    private readonly Func<T, bool> _predicate;

    public All(Func<T, bool> predicate) => (_predicate, _all) = (predicate, true);

    private bool _all;

    bool IProcessStream<T, T, bool>.GetResult(ref StreamState<T> state) => _all;

    bool IProcessStream<T, T>.ProcessNext(ref StreamState<T> state, in T input) =>
        _all = _predicate(input);
}

