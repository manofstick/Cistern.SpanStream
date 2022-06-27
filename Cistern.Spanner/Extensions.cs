using Cistern.Spanner.Maths;
using Cistern.Spanner.Roots;
using Cistern.Spanner.Terminators;
using Cistern.Spanner.Transforms;
using System.Buffers;
using System.Collections.Immutable;

namespace Cistern.Spanner;

using SumDecimal         = Sum        <decimal, decimal, decimal, OpsDecimal>;
using SumDecimalNullable = SumNullable<decimal, decimal, decimal, OpsDecimal>;
using SumDouble          = Sum        <double,  double,  double,  OpsDouble>;
using SumDoubleNullable  = SumNullable<double,  double,  double,  OpsDouble>;
using SumFloat           = Sum        <float,   double,  float,   OpsFloat>;
using SumFloatNullable   = SumNullable<float,   double,  float,   OpsFloat>;
using SumInt             = Sum        <int,     long,    double,  OpsInt>;
using SumIntNullable     = SumNullable<int,     long,    double,  OpsInt>;
using SumLong            = Sum        <long,    long,    double,  OpsLong>;
using SumLongNullable    = SumNullable<long,    long,    double,  OpsLong>;

public static class Extensions
{
    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this Span<T> span) => span;
    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this Memory<T> memory) => memory.Span;
    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this ReadOnlyMemory<T> memory) => memory.Span;
    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this T[] array) => array;
    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this ImmutableArray<T> array) => array.AsSpan();

    // -----

    public static TInitial Aggregate<TInitial>(this ReadOnlySpan<TInitial> source, Func<TInitial, TInitial, TInitial> func) =>
        Root<TInitial>.Execute<TInitial, Aggregate<TInitial>>(new(func), source);
    public static TAccumulate Aggregate<TInitial, TAccumulate>(this ReadOnlySpan<TInitial> source, TAccumulate seed, Func<TAccumulate, TInitial, TAccumulate> func) =>
        Root<TInitial>.Execute<TAccumulate, Aggregate<TInitial, TAccumulate>>(new(func, seed), source);
    public static TResult Aggregate<TInitial, TAccumulate, TResult>(this ReadOnlySpan<TInitial> source, TAccumulate seed, Func<TAccumulate, TInitial, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) =>
        Root<TInitial>.Execute<TResult, Aggregate<TInitial, TAccumulate, TResult>>(new(func, seed, resultSelector), source);
    public static TCurrent Aggregate<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TCurrent, TCurrent> func)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        source.Execute<TCurrent, Aggregate<TCurrent>>(new(func));
    public static TAccumulate Aggregate<TInitial, TCurrent, TNode, TAccumulate>(this SpanHost<TInitial, TCurrent, TNode> source, TAccumulate seed, Func<TAccumulate, TCurrent, TAccumulate> func)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        source.Execute<TAccumulate, Aggregate<TCurrent, TAccumulate>>(new(func, seed));
    public static TResult Aggregate<TInitial, TCurrent, TNode, TAccumulate, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, TAccumulate seed, Func<TAccumulate, TCurrent, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        source.Execute<TResult, Aggregate<TCurrent, TAccumulate, TResult>>(new(func, seed, resultSelector));

    //- [ ] `bool All<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `bool Any<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `bool Any<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`

    public static SpanHost<TInitial, TInitial, Append<TInitial, TInitial, Root<TInitial>>> Append<TInitial>(this ReadOnlySpan<TInitial> source, TInitial item) =>
        new(source, new(ref Root<TInitial>.Instance, item));
    public static SpanHost<TInitial, TCurrent, Append<TInitial, TCurrent, TNode>> Append<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, TCurrent item)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, item));

    //- [ ] `IEnumerable<TInitial> AsEnumerable<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `double Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
    //- [ ] `double Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
    //- [ ] `double? Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
    //- [ ] `float Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
    //- [ ] `double? Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
    //- [ ] `float? Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
    //- [ ] `double Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
    //- [ ] `double? Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
    //- [ ] `decimal Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
    //- [ ] `decimal? Average<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
    //- [ ] `float? Average(this IEnumerable<float?> source);`
    //- [ ] `double? Average(this IEnumerable<long?> source);`
    //- [ ] `double? Average(this IEnumerable<int?> source);`
    //- [ ] `double? Average(this IEnumerable<double?> source);`
    //- [ ] `decimal? Average(this IEnumerable<decimal?> source);`
    //- [ ] `double Average(this IEnumerable<long> source);`
    //- [ ] `double Average(this IEnumerable<int> source);`
    //- [ ] `double Average(this IEnumerable<double> source);`
    //- [ ] `decimal Average(this IEnumerable<decimal> source);`
    //- [ ] `float Average(this IEnumerable<float> source);`
    //- [ ] `IEnumerable<TResult> Cast<TResult>(this IEnumerable source);`
    //- [ ] `IEnumerable<TInitial[]> Chunk<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int size);`
    //- [ ] `IEnumerable<TInitial> Concat<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
    //- [ ] `bool Contains<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial value, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `bool Contains<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial value);`
    //- [ ] `int Count<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `int Count<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `IEnumerable<TInitial?> DefaultIfEmpty<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `IEnumerable<TInitial> DefaultIfEmpty<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
    //- [ ] `IEnumerable<TInitial> Distinct<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `IEnumerable<TInitial> Distinct<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `IEnumerable<TInitial> DistinctBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<TInitial> DistinctBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `TInitial ElementAt<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Index index);`
    //- [ ] `TInitial ElementAt<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int index);`
    //- [ ] `TInitial? ElementAtOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Index index);`
    //- [ ] `TInitial? ElementAtOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int index);`
    //- [ ] `IEnumerable<TResult> Empty<TResult>();`
    //- [ ] `IEnumerable<TInitial> Except<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
    //- [ ] `IEnumerable<TInitial> Except<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `IEnumerable<TInitial> ExceptBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<TInitial> ExceptBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `TInitial First<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial First<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `TInitial? FirstOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial FirstOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
    //- [ ] `TInitial? FirstOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `TInitial FirstOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
    //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TElement, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TElement, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector);`
    //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TKey, IEnumerable<TInitial>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TKey, IEnumerable<TInitial>, TResult> resultSelector);`
    //- [ ] `IEnumerable<IGrouping<TKey, TInitial>> GroupBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<IGrouping<TKey, TElement>> GroupBy<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector);`
    //- [ ] `IEnumerable<IGrouping<TKey, TInitial>> GroupBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<IGrouping<TKey, TElement>> GroupBy<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector);`
    //- [ ] `IEnumerable<TInitial> Intersect<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `IEnumerable<TInitial> Intersect<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
    //- [ ] `IEnumerable<TInitial> IntersectBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<TInitial> IntersectBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector);`
    //- [ ] `IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `TInitial Last<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial Last<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `TInitial? LastOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial LastOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
    //- [ ] `TInitial? LastOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `TInitial LastOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
    //- [ ] `long LongCount<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `long LongCount<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `long Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
    //- [ ] `decimal Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
    //- [ ] `double Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
    //- [ ] `int Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
    //- [ ] `decimal? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
    //- [ ] `TInitial? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, IComparer<TInitial>? comparer);`
    //- [ ] `int? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
    //- [ ] `long? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
    //- [ ] `float? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
    //- [ ] `TResult? Max<TInitial, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TResult> selector);`
    //- [ ] `double? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
    //- [ ] `TInitial? Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `float Max<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
    //- [ ] `float Max(this IEnumerable<float> source);`
    //- [ ] `float? Max(this IEnumerable<float?> source);`
    //- [ ] `long? Max(this IEnumerable<long?> source);`
    //- [ ] `int? Max(this IEnumerable<int?> source);`
    //- [ ] `double? Max(this IEnumerable<double?> source);`
    //- [ ] `decimal? Max(this IEnumerable<decimal?> source);`
    //- [ ] `long Max(this IEnumerable<long> source);`
    //- [ ] `int Max(this IEnumerable<int> source);`
    //- [ ] `double Max(this IEnumerable<double> source);`
    //- [ ] `decimal Max(this IEnumerable<decimal> source);`
    //- [ ] `TInitial? MaxBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `TInitial? MaxBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
    //- [ ] `decimal Min(this IEnumerable<decimal> source);`
    //- [ ] `TResult? Min<TInitial, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TResult> selector);`
    //- [ ] `float Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
    //- [ ] `float? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
    //- [ ] `int? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
    //- [ ] `double? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
    //- [ ] `decimal? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
    //- [ ] `long Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
    //- [ ] `int Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
    //- [ ] `decimal Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
    //- [ ] `TInitial? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, IComparer<TInitial>? comparer);`
    //- [ ] `TInitial? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `long? Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
    //- [ ] `double Min<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
    //- [ ] `float Min(this IEnumerable<float> source);`
    //- [ ] `float? Min(this IEnumerable<float?> source);`
    //- [ ] `long? Min(this IEnumerable<long?> source);`
    //- [ ] `int? Min(this IEnumerable<int?> source);`
    //- [ ] `double? Min(this IEnumerable<double?> source);`
    //- [ ] `decimal? Min(this IEnumerable<decimal?> source);`
    //- [ ] `double Min(this IEnumerable<double> source);`
    //- [ ] `long Min(this IEnumerable<long> source);`
    //- [ ] `int Min(this IEnumerable<int> source);`
    //- [ ] `TInitial? MinBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
    //- [ ] `TInitial? MinBy<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<TResult> OfType<TResult>(this IEnumerable source);`

    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, SingleKeyWithComparer<TKey>, SingleKeyFactoryWithComparer<TCurrent, TKey>, TNode>> OrderBy<TInitial, TCurrent, TKey, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TKey> getKey, IComparer<TKey> comparer, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, new (getKey, comparer), stackElementCount, maybeArrayPool));
    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, TKey, SingleKeyFactory<TCurrent, TKey>, TNode>> OrderBy<TInitial, TCurrent, TKey, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TKey> getKey, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, new(getKey), stackElementCount, maybeArrayPool));
    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, SingleKeyWithComparer<string>, SingleKeyFactoryWithComparer<TCurrent, string>, TNode>> OrderBy<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, string> getKey, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, new(getKey, StringComparer.CurrentCulture), stackElementCount, maybeArrayPool));

    //- [ ] `IOrderedEnumerable<TInitial> OrderByDescending<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `IOrderedEnumerable<TInitial> OrderByDescending<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
    //- [ ] `IEnumerable<TInitial> Prepend<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial element);`
    //- [ ] `IEnumerable<int> Range(int start, int count);`
    //- [ ] `IEnumerable<TResult> Repeat<TResult>(TResult element, int count);`

    public static SpanHost<TInitial, TInitial, RootReverse<TInitial>> Reverse<TInitial>(this ReadOnlySpan<TInitial> source)
        => new(source, new());
    public static SpanHost<TInitial, TCurrent, Reverse<TInitial, TCurrent, TNode>> Reverse<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, stackElementCount, maybeArrayPool));
    public static SpanHost<TInitial, TCurrent, TNode> Reverse<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, Reverse<TInitial, TCurrent, TNode>> source)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, in source.Node.Node);

    public static SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> Select<TInitial, TCurrent>(this ReadOnlySpan<TInitial> span, Func<TInitial, TCurrent> selector) =>
        new(span, new(selector));
    public static SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> Select<TInitial, TCurrent>(this SpanHost<TInitial, TInitial, Root<TInitial>> source, Func<TInitial, TCurrent> selector) =>
        new(in source.Span, new(selector));
    public static SpanHost<TCurrent, TNext, WhereSelectRoot<TCurrent, TNext>> Select<TCurrent, TNext>(this SpanHost<TCurrent, TCurrent, WhereRoot<TCurrent>> source, Func<TCurrent, TNext> selector) =>
        new(in source.Span, new(source.Node.Predicate, selector));
    public static SpanHost<TInitial, TNext, WhereSelect<TInitial, TCurrent, TNext, TNode>> Select<TInitial, TCurrent, TNext, TNode>(this SpanHost<TInitial, TCurrent, Where<TInitial, TCurrent, TNode>> source, Func<TCurrent, TNext> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node.Node, source.Node.Predicate, selector));
    public static SpanHost<TInitial, TNext, Select<TInitial, TCurrent, TNext, TNode>> Select<TInitial, TCurrent, TNext, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TNext> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, selector));

    //- [ ] `IEnumerable<TResult> Select<TInitial, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, TResult> selector);`

    //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, IEnumerable<TResult>> selector);`
    //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TCollection, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, IEnumerable<TCollection>> collectionSelector, Func<TInitial, TCollection, TResult> resultSelector);`
    //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TCollection, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, IEnumerable<TCollection>> collectionSelector, Func<TInitial, TCollection, TResult> resultSelector);`
    //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TResult>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, IEnumerable<TResult>> selector);`
    //- [ ] `bool SequenceEqual<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
    //- [ ] `bool SequenceEqual<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `TInitial Single<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial Single<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `TInitial SingleOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
    //- [ ] `TInitial SingleOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
    //- [ ] `TInitial? SingleOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `TInitial? SingleOrDefault<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `IEnumerable<TInitial> Skip<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int count);`
    //- [ ] `IEnumerable<TInitial> SkipLast<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int count);`
    //- [ ] `IEnumerable<TInitial> SkipWhile<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `IEnumerable<TInitial> SkipWhile<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`

    public static int Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, int> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static long Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, long> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static decimal? Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, decimal?> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static long? Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, long?> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static int? Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, int?> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static double Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, double> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static float? Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, float?> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static float Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, float> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static double? Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, double?> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();
    public static decimal Sum<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, decimal> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> => source.Select(selector).Sum();

    public static long? Sum<TInitial, TNode>(this SpanHost<TInitial, long?, TNode> source)
        where TNode : struct, IStreamNode<TInitial, long?> => source.Execute<long?, SumLongNullable>(new());
    public static int? Sum<TInitial, TNode>(this SpanHost<TInitial, int?, TNode> source)
        where TNode : struct, IStreamNode<TInitial, int?> => source.Execute<int?, SumIntNullable>(new());
    public static double? Sum<TInitial, TNode>(this SpanHost<TInitial, double?, TNode> source)
        where TNode : struct, IStreamNode<TInitial, double?> => source.Execute<double?, SumDoubleNullable>(new());
    public static decimal? Sum<TInitial, TNode>(this SpanHost<TInitial, decimal?, TNode> source)
        where TNode : struct, IStreamNode<TInitial, decimal?> => source.Execute<decimal?, SumDecimalNullable>(new());
    public static float? Sum<TInitial, TNode>(this SpanHost<TInitial, float?, TNode> source)
        where TNode : struct, IStreamNode<TInitial, float?> => source.Execute<float?, SumFloatNullable>(new());

    public static long Sum<TInitial, TNode>(this SpanHost<TInitial, long, TNode> source)
        where TNode : struct, IStreamNode<TInitial, long> => source.Execute<long, SumLong>(new());
    public static int Sum<TInitial, TNode>(this SpanHost<TInitial, int, TNode> source)
        where TNode : struct, IStreamNode<TInitial, int> => source.Execute<int, SumInt>(new ());
    public static double Sum<TInitial, TNode>(this SpanHost<TInitial, double, TNode> source)
        where TNode : struct, IStreamNode<TInitial, double> => source.Execute<double, SumDouble>(new());
    public static decimal Sum<TInitial, TNode>(this SpanHost<TInitial, decimal, TNode> source)
        where TNode : struct, IStreamNode<TInitial, decimal> => source.Execute<decimal, SumDecimal>(new());
    public static float Sum<TInitial, TNode>(this SpanHost<TInitial, float, TNode> source)
        where TNode : struct, IStreamNode<TInitial, float> => source.Execute<float, SumFloat>(new());

    public static long Sum(this ReadOnlySpan<long> source, SIMDOptions options = SIMDOptions.Fastest) => SumLong.SimdSum(source, options);
    public static int Sum(this ReadOnlySpan<int> source, SIMDOptions options = SIMDOptions.Fastest) => SumInt.SimdSum(source, options);
    public static double Sum(this ReadOnlySpan<double> source, SIMDOptions options = SIMDOptions.Fastest) => SumDouble.SimdSum(source, options);
    public static decimal Sum(this ReadOnlySpan<decimal> source, SIMDOptions options = SIMDOptions.Fastest) => SumDecimal.SimdSum(source, options);
    public static float Sum(this ReadOnlySpan<float> source, SIMDOptions options = SIMDOptions.Fastest) => SumFloat.SimdSum(source, options);

    public static long? Sum(this ReadOnlySpan<long?> source) => Root<long?>.Execute<long?, SumLongNullable>(new(), source);
    public static int? Sum(this ReadOnlySpan<int?> source) => Root<int?>.Execute<int?, SumIntNullable>(new(), source);
    public static double? Sum(this ReadOnlySpan<double?> source) => Root<double?>.Execute<double?, SumDoubleNullable>(new(), source);
    public static decimal? Sum(this ReadOnlySpan<decimal?> source) => Root<decimal?>.Execute<decimal?, SumDecimalNullable>(new(), source);
    public static float? Sum(this ReadOnlySpan<float?> source) => Root<float?>.Execute<float?, SumFloatNullable>(new(), source);

    //- [ ] `IEnumerable<TInitial> Take<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Range range);`
    //- [ ] `IEnumerable<TInitial> Take<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int count);`
    //- [ ] `IEnumerable<TInitial> TakeLast<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, int count);`
    //- [ ] `IEnumerable<TInitial> TakeWhile<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
    //- [ ] `IEnumerable<TInitial> TakeWhile<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`


    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, CompositeKey<TKey, TCurrentKey>, CompositeKeyFactory<TCurrent, TKey, TCurrentKey, TCurrentKeyFactory>, TNode>> ThenBy<TInitial, TCurrent, TKey, TCurrentKey, TCurrentKeyFactory, TNode>(this SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, TCurrentKey, TCurrentKeyFactory, TNode>> source, Func<TCurrent, TKey> getKey, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        where TCurrentKeyFactory : struct, IKeyFactory<TCurrent, TCurrentKey> =>
        new(in source.Span, new(ref source.Node.Node, new(getKey, source.Node._keyFactory), stackElementCount, maybeArrayPool));
    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, CompositeKeyWithComparer<TKey, TCurrentKey>, CompositeKeyFactoryWithComparer<TCurrent, TKey, TCurrentKey, TCurrentKeyFactory>, TNode>> ThenBy<TInitial, TCurrent, TKey, TCurrentKey, TCurrentKeyFactory, TNode>(this SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, TCurrentKey, TCurrentKeyFactory, TNode>> source, Func<TCurrent, TKey> getKey, IComparer<TKey> comparer, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        where TCurrentKeyFactory : struct, IKeyFactory<TCurrent, TCurrentKey> =>
        new(in source.Span, new(ref source.Node.Node, new(getKey, comparer, source.Node._keyFactory), stackElementCount, maybeArrayPool));
#if HANDLE_SPECIFIC_STRING_COMPARER
    public static SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, CompositeKeyWithComparer<string, TCurrentKey>, CompositeKeyFactoryWithComparer<TCurrent, string, TCurrentKey, TCurrentKeyFactory>, TNode>> ThenBy<TInitial, TCurrent, TCurrentKey, TCurrentKeyFactory, TNode>(this SpanHost<TInitial, TCurrent, OrderBy<TInitial, TCurrent, TCurrentKey, TCurrentKeyFactory, TNode>> source, Func<TCurrent, string> getKey, int stackElementCount = 100, ArrayPool<TCurrent>? maybeArrayPool = null)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        where TCurrentKeyFactory : struct, IKeyFactory<TCurrent, TCurrentKey> =>
        new(in source.Span, new(ref source.Node.Node, new(getKey, StringComparer.CurrentCulture, source.Node._keyFactory), stackElementCount, maybeArrayPool));
#endif

    //- [ ] `IOrderedEnumerable<TInitial> ThenByDescending<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `IOrderedEnumerable<TInitial> ThenByDescending<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`

    public static TCurrent[] ToArray<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, int stackElementCount, ArrayPool<TCurrent>? maybeArrayPool)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        => ToArray<TCurrent>.Execute(in source.Span, ref source.Node, stackElementCount, maybeArrayPool);

    public static TCurrent[] ToArray<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, int stackElementCount = 100, bool useSharedPool = false)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        => source.ToArray(stackElementCount, useSharedPool ? ArrayPool<TCurrent>.Shared : null);

    public static TCurrent[] ToArray<TInitial, TCurrent>(this SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> source)
        => Iterator.SelectToArray(in source.Span, source.Node.Selector);

    //- [ ] `Dictionary<TKey, TInitial> ToDictionary<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector) where TKey : notnull;`
    //- [ ] `Dictionary<TKey, TInitial> ToDictionary<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer) where TKey : notnull;`
    //- [ ] `Dictionary<TKey, TElement> ToDictionary<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector) where TKey : notnull;`
    //- [ ] `Dictionary<TKey, TElement> ToDictionary<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull;`
    //- [ ] `HashSet<TInitial> ToHashSet<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `HashSet<TInitial> ToHashSet<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `List<TInitial> ToList<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source);`
    //- [ ] `ILookup<TKey, TElement> ToLookup<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `ILookup<TKey, TElement> ToLookup<TInitial, TKey, TElement>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector);`
    //- [ ] `ILookup<TKey, TInitial> ToLookup<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
    //- [ ] `ILookup<TKey, TInitial> ToLookup<TInitial, TKey>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
    //- [ ] `bool TryGetNonEnumeratedCount<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, out int count);`
    //- [ ] `IEnumerable<TInitial> Union<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
    //- [ ] `IEnumerable<TInitial> Union<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
    //- [ ] `IEnumerable<TInitial> UnionBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, Func<TInitial, TKey> keySelector);`
    //- [ ] `IEnumerable<TInitial> UnionBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`

    public static SpanHost<TInitial, TInitial, WhereRoot<TInitial>> Where<TInitial>(this ReadOnlySpan<TInitial> span, Func<TInitial, bool> predicate) =>
        new(in span, new(predicate));
    public static SpanHost<TInitial, TInitial, WhereRoot<TInitial>> Where<TInitial>(this SpanHost<TInitial, TInitial, Root<TInitial>> source, Func<TInitial, bool> predicate) =>
        new(in source.Span, new(predicate));
    public static SpanHost<TInitial, TCurrent, SelectWhereRoot<TInitial, TCurrent>> Where<TInitial, TCurrent>(this SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> source, Func<TCurrent, bool> predicate) =>
        new(in source.Span, new(source.Node.Selector, predicate));
    public static SpanHost<TInitial, TNext, SelectWhere<TInitial, TCurrent, TNext, TNode>> Where<TInitial, TCurrent, TNext, TNode>(this SpanHost<TInitial, TNext, Select<TInitial, TCurrent, TNext, TNode>> source, Func<TNext, bool> predicate)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node.Node, source.Node.Selector, predicate));
    public static SpanHost<TInitial, TCurrent, Where<TInitial, TCurrent, TNode>> Where<TInitial, TCurrent, TNode>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, bool> predicate)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node, predicate));

    //- [ ] `IEnumerable<TInitial> Where<TInitial>(this SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`
    //- [ ] `IEnumerable<(TFirst First, TSecond Second, TThird Third)> Zip<TFirst, TSecond, TThird>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third);`
    //- [ ] `IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second);`
    //- [ ] `IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector);`

}
