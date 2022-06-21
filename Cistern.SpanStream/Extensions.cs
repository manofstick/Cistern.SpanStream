using Cistern.SpanStream.Roots;
using Cistern.SpanStream.Terminators;
using Cistern.SpanStream.Transforms;
using System.Buffers;
using System.Collections.Immutable;

namespace Cistern.SpanStream
{
    public static class Extensions
    {
        public static ReadOnlySpan<T> ToReadOnlySpan<T>(this in Span<T> span) => span;
        public static ReadOnlySpan<T> ToReadOnlySpan<T>(this in Memory<T> memory) => memory.Span;
        public static ReadOnlySpan<T> ToReadOnlySpan<T>(this in ReadOnlyMemory<T> memory) => memory.Span;
        public static ReadOnlySpan<T> ToReadOnlySpan<T>(this T[] array) => array;
        public static ReadOnlySpan<T> ToReadOnlySpan<T>(this ImmutableArray<T> array) => array.AsSpan();

        // -----

        public static TCurrent Aggregate<TInitial, TCurrent, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TCurrent, TCurrent> func)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            source.Execute<TCurrent, Aggregate<TCurrent>>(new(func));
        public static TAccumulate Aggregate<TInitial, TCurrent, TNode, TAccumulate>(this in SpanHost<TInitial, TCurrent, TNode> source, TAccumulate seed, Func<TAccumulate, TCurrent, TAccumulate> func)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            source.Execute<TAccumulate, Aggregate<TCurrent, TAccumulate>>(new(func, seed));
        public static TResult Aggregate<TInitial, TCurrent, TNode, TAccumulate, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, TAccumulate seed, Func<TAccumulate, TCurrent, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            source.Execute<TResult, Aggregate<TCurrent, TAccumulate, TResult>>(new(func, seed, resultSelector));

        //- [ ] `bool All<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `bool Any<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `bool Any<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`

        public static SpanHost<TInitial, TCurrent, Append<TInitial, TCurrent, TNode>> Append<TInitial, TCurrent, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, TCurrent item)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            new(source.Span, new(in source.Node, item));

        //- [ ] `IEnumerable<TInitial> AsEnumerable<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `double Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
        //- [ ] `double Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
        //- [ ] `double? Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
        //- [ ] `float Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
        //- [ ] `double? Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
        //- [ ] `float? Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
        //- [ ] `double Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
        //- [ ] `double? Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
        //- [ ] `decimal Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
        //- [ ] `decimal? Average<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
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
        //- [ ] `IEnumerable<TInitial[]> Chunk<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int size);`
        //- [ ] `IEnumerable<TInitial> Concat<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
        //- [ ] `bool Contains<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial value, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `bool Contains<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial value);`
        //- [ ] `int Count<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `int Count<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `IEnumerable<TInitial?> DefaultIfEmpty<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `IEnumerable<TInitial> DefaultIfEmpty<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
        //- [ ] `IEnumerable<TInitial> Distinct<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `IEnumerable<TInitial> Distinct<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `IEnumerable<TInitial> DistinctBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<TInitial> DistinctBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `TInitial ElementAt<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Index index);`
        //- [ ] `TInitial ElementAt<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int index);`
        //- [ ] `TInitial? ElementAtOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Index index);`
        //- [ ] `TInitial? ElementAtOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int index);`
        //- [ ] `IEnumerable<TResult> Empty<TResult>();`
        //- [ ] `IEnumerable<TInitial> Except<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
        //- [ ] `IEnumerable<TInitial> Except<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `IEnumerable<TInitial> ExceptBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<TInitial> ExceptBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `TInitial First<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial First<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `TInitial? FirstOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial FirstOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
        //- [ ] `TInitial? FirstOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `TInitial FirstOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
        //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TElement, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TElement, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector);`
        //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TKey, IEnumerable<TInitial>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TResult> GroupBy<TInitial, TKey, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TKey, IEnumerable<TInitial>, TResult> resultSelector);`
        //- [ ] `IEnumerable<IGrouping<TKey, TInitial>> GroupBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<IGrouping<TKey, TElement>> GroupBy<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector);`
        //- [ ] `IEnumerable<IGrouping<TKey, TInitial>> GroupBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<IGrouping<TKey, TElement>> GroupBy<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector);`
        //- [ ] `IEnumerable<TInitial> Intersect<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `IEnumerable<TInitial> Intersect<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
        //- [ ] `IEnumerable<TInitial> IntersectBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<TInitial> IntersectBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TKey> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector);`
        //- [ ] `IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `TInitial Last<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial Last<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `TInitial? LastOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial LastOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
        //- [ ] `TInitial? LastOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `TInitial LastOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
        //- [ ] `long LongCount<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `long LongCount<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `long Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
        //- [ ] `decimal Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
        //- [ ] `double Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
        //- [ ] `int Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
        //- [ ] `decimal? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
        //- [ ] `TInitial? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, IComparer<TInitial>? comparer);`
        //- [ ] `int? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
        //- [ ] `long? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
        //- [ ] `float? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
        //- [ ] `TResult? Max<TInitial, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TResult> selector);`
        //- [ ] `double? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
        //- [ ] `TInitial? Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `float Max<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
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
        //- [ ] `TInitial? MaxBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `TInitial? MaxBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
        //- [ ] `decimal Min(this IEnumerable<decimal> source);`
        //- [ ] `TResult? Min<TInitial, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TResult> selector);`
        //- [ ] `float Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
        //- [ ] `float? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
        //- [ ] `int? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
        //- [ ] `double? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
        //- [ ] `decimal? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
        //- [ ] `long Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
        //- [ ] `int Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
        //- [ ] `decimal Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
        //- [ ] `TInitial? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, IComparer<TInitial>? comparer);`
        //- [ ] `TInitial? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `long? Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
        //- [ ] `double Min<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
        //- [ ] `float Min(this IEnumerable<float> source);`
        //- [ ] `float? Min(this IEnumerable<float?> source);`
        //- [ ] `long? Min(this IEnumerable<long?> source);`
        //- [ ] `int? Min(this IEnumerable<int?> source);`
        //- [ ] `double? Min(this IEnumerable<double?> source);`
        //- [ ] `decimal? Min(this IEnumerable<decimal?> source);`
        //- [ ] `double Min(this IEnumerable<double> source);`
        //- [ ] `long Min(this IEnumerable<long> source);`
        //- [ ] `int Min(this IEnumerable<int> source);`
        //- [ ] `TInitial? MinBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
        //- [ ] `TInitial? MinBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<TResult> OfType<TResult>(this IEnumerable source);`
        //- [ ] `IOrderedEnumerable<TInitial> OrderBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
        //- [ ] `IOrderedEnumerable<TInitial> OrderBy<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IOrderedEnumerable<TInitial> OrderByDescending<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IOrderedEnumerable<TInitial> OrderByDescending<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
        //- [ ] `IEnumerable<TInitial> Prepend<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial element);`
        //- [ ] `IEnumerable<int> Range(int start, int count);`
        //- [ ] `IEnumerable<TResult> Repeat<TResult>(TResult element, int count);`
        //- [ ] `IEnumerable<TInitial> Reverse<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`

        public static SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> Select<TInitial, TCurrent>(this in ReadOnlySpan<TInitial> span, Func<TInitial, TCurrent> selector) =>
            new(span, new(selector));
        public static SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> Select<TInitial, TCurrent>(this in SpanHost<TInitial, TInitial, Root<TInitial>> source, Func<TInitial, TCurrent> selector) =>
            new(source.Span, new(selector));
        public static SpanHost<TCurrent, TNext, WhereSelectRoot<TCurrent, TNext>> Select<TCurrent, TNext>(this in SpanHost<TCurrent, TCurrent, WhereRoot<TCurrent>> source, Func<TCurrent, TNext> selector) =>
            new(source.Span, new(source.Node.Predicate, selector));
        public static SpanHost<TInitial, TNext, WhereSelect<TInitial, TCurrent, TNext, TNode>> Select<TInitial, TCurrent, TNext, TNode>(this in SpanHost<TInitial, TCurrent, Where<TInitial, TCurrent, TNode>> source, Func<TCurrent, TNext> selector)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            new(source.Span, new(in source.Node.Node, source.Node.Predicate, selector));
        public static SpanHost<TInitial, TNext, Select<TInitial, TCurrent, TNext, TNode>> Select<TInitial, TCurrent, TNext, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, TNext> selector)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            new(source.Span, new(in source.Node, selector));

        //- [ ] `IEnumerable<TResult> Select<TInitial, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, TResult> selector);`

        //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, IEnumerable<TResult>> selector);`
        //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TCollection, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, IEnumerable<TCollection>> collectionSelector, Func<TInitial, TCollection, TResult> resultSelector);`
        //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TCollection, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, IEnumerable<TCollection>> collectionSelector, Func<TInitial, TCollection, TResult> resultSelector);`
        //- [ ] `IEnumerable<TResult> SelectMany<TInitial, TResult>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, IEnumerable<TResult>> selector);`
        //- [ ] `bool SequenceEqual<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
        //- [ ] `bool SequenceEqual<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `TInitial Single<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial Single<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `TInitial SingleOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate, TInitial defaultValue);`
        //- [ ] `TInitial SingleOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, TInitial defaultValue);`
        //- [ ] `TInitial? SingleOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `TInitial? SingleOrDefault<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `IEnumerable<TInitial> Skip<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int count);`
        //- [ ] `IEnumerable<TInitial> SkipLast<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int count);`
        //- [ ] `IEnumerable<TInitial> SkipWhile<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `IEnumerable<TInitial> SkipWhile<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`
        //- [ ] `int Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int> selector);`
        //- [ ] `long Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long> selector);`
        //- [ ] `decimal? Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal?> selector);`
        //- [ ] `long? Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, long?> selector);`
        //- [ ] `int? Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int?> selector);`
        //- [ ] `double Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double> selector);`
        //- [ ] `float? Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float?> selector);`
        //- [ ] `float Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, float> selector);`
        //- [ ] `double? Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, double?> selector);`
        //- [ ] `decimal Sum<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, decimal> selector);`
        //- [ ] `long? Sum(this IEnumerable<long?> source);`
        //- [ ] `float? Sum(this IEnumerable<float?> source);`
        //- [ ] `int? Sum(this IEnumerable<int?> source);`
        //- [ ] `double? Sum(this IEnumerable<double?> source);`
        //- [ ] `decimal? Sum(this IEnumerable<decimal?> source);`
        //- [ ] `long Sum(this IEnumerable<long> source);`
        //- [ ] `int Sum(this IEnumerable<int> source);`
        //- [ ] `double Sum(this IEnumerable<double> source);`
        //- [ ] `decimal Sum(this IEnumerable<decimal> source);`
        //- [ ] `float Sum(this IEnumerable<float> source);`
        //- [ ] `IEnumerable<TInitial> Take<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Range range);`
        //- [ ] `IEnumerable<TInitial> Take<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int count);`
        //- [ ] `IEnumerable<TInitial> TakeLast<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, int count);`
        //- [ ] `IEnumerable<TInitial> TakeWhile<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, bool> predicate);`
        //- [ ] `IEnumerable<TInitial> TakeWhile<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`
        //- [ ] `IOrderedEnumerable<TInitial> ThenBy<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IOrderedEnumerable<TInitial> ThenBy<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`
        //- [ ] `IOrderedEnumerable<TInitial> ThenByDescending<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `IOrderedEnumerable<TInitial> ThenByDescending<TInitial, TKey>(this IOrderedEnumerable<TInitial> source, Func<TInitial, TKey> keySelector, IComparer<TKey>? comparer);`

        public static TCurrent[] ToArray<TInitial, TCurrent, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, int stackElementCount, ArrayPool<TCurrent>? maybeArrayPool)
            where TNode : struct, IStreamNode<TInitial, TCurrent>
        {
            var maybeSize = source.TryGetSize(out var upperBound);

            if (upperBound == 0)
                return Array.Empty<TCurrent>();

            if (maybeSize.HasValue)
                return source.Execute<TCurrent[], ToArrayKnownSize<TCurrent>>(new(new TCurrent[maybeSize.Value]));

            return source.Execute<TCurrent[], ToArray<TCurrent>>(new(upperBound, maybeArrayPool), Math.Min(upperBound, stackElementCount));
        }

        public static TCurrent[] ToArray<TInitial, TCurrent, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, int stackElementCount = 100, bool useSharedPool = false)
            where TNode : struct, IStreamNode<TInitial, TCurrent>
            => source.ToArray(stackElementCount, useSharedPool ? ArrayPool<TCurrent>.Shared : null);

        public static TCurrent[] ToArray<TInitial, TCurrent>(this in SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> source)
            => Iterator.SelectToArray(source.Span, source.Node.Selector);

        //- [ ] `Dictionary<TKey, TInitial> ToDictionary<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector) where TKey : notnull;`
        //- [ ] `Dictionary<TKey, TInitial> ToDictionary<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer) where TKey : notnull;`
        //- [ ] `Dictionary<TKey, TElement> ToDictionary<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector) where TKey : notnull;`
        //- [ ] `Dictionary<TKey, TElement> ToDictionary<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull;`
        //- [ ] `HashSet<TInitial> ToHashSet<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `HashSet<TInitial> ToHashSet<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `List<TInitial> ToList<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source);`
        //- [ ] `ILookup<TKey, TElement> ToLookup<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `ILookup<TKey, TElement> ToLookup<TInitial, TKey, TElement>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, Func<TInitial, TElement> elementSelector);`
        //- [ ] `ILookup<TKey, TInitial> ToLookup<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector);`
        //- [ ] `ILookup<TKey, TInitial> ToLookup<TInitial, TKey>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`
        //- [ ] `bool TryGetNonEnumeratedCount<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, out int count);`
        //- [ ] `IEnumerable<TInitial> Union<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second);`
        //- [ ] `IEnumerable<TInitial> Union<TInitial>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, IEqualityComparer<TInitial>? comparer);`
        //- [ ] `IEnumerable<TInitial> UnionBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, Func<TInitial, TKey> keySelector);`
        //- [ ] `IEnumerable<TInitial> UnionBy<TInitial, TKey>(this IEnumerable<TInitial> first, IEnumerable<TInitial> second, Func<TInitial, TKey> keySelector, IEqualityComparer<TKey>? comparer);`

        public static SpanHost<TInitial, TInitial, WhereRoot<TInitial>> Where<TInitial>(this in ReadOnlySpan<TInitial> span, Func<TInitial, bool> predicate) =>
            new(span, new(predicate));
        public static SpanHost<TInitial, TInitial, WhereRoot<TInitial>> Where<TInitial>(this in SpanHost<TInitial, TInitial, Root<TInitial>> source, Func<TInitial, bool> predicate) =>
            new(source.Span, new(predicate));
        public static SpanHost<TInitial, TCurrent, SelectWhereRoot<TInitial, TCurrent>> Where<TInitial, TCurrent>(this in SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>> source, Func<TCurrent, bool> predicate) =>
            new(source.Span, new(source.Node.Selector, predicate));
        public static SpanHost<TInitial, TNext, SelectWhere<TInitial, TCurrent, TNext, TNode>> Where<TInitial, TCurrent, TNext, TNode>(this in SpanHost<TInitial, TNext, Select<TInitial, TCurrent, TNext, TNode>> source, Func<TNext, bool> predicate)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            new(source.Span, new(in source.Node.Node, source.Node.Selector, predicate));
        public static SpanHost<TInitial, TCurrent, Where<TInitial, TCurrent, TNode>> Where<TInitial, TCurrent, TNode>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TCurrent, bool> predicate)
            where TNode : struct, IStreamNode<TInitial, TCurrent> =>
            new(source.Span, new(in source.Node, predicate));

        //- [ ] `IEnumerable<TInitial> Where<TInitial>(this in SpanHost<TInitial, TCurrent, TNode> source, Func<TInitial, int, bool> predicate);`
        //- [ ] `IEnumerable<(TFirst First, TSecond Second, TThird Third)> Zip<TFirst, TSecond, TThird>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third);`
        //- [ ] `IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second);`
        //- [ ] `IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector);`

    }
}
