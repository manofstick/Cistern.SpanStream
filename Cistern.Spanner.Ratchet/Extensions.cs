using Cistern.Spanner.Ratchet.Roots;
using Cistern.Spanner.Ratchet.Transforms;
using Cistern.Spanner.Roots;
using Cistern.Spanner.Transforms;

namespace Cistern.Spanner.Ratchet;

public static class Extensions
{
    public static int Count<TCurrent, TContext>(this SpanHost<TCurrent, TCurrent, WhereRoot<TCurrent>, TContext> source)
         => IteratorEx.WhereCount<TCurrent, TContext>(in source.Span, source.Node.Predicate);

    public static SpanHost<TInitial, TCurrent, AppendN<TInitial, TCurrent, AppendItem<TCurrent>, TNode>, TContext> Append<TInitial, TCurrent, TNode, TContext>(this SpanHost<TInitial, TCurrent, Append<TInitial, TCurrent, TNode>, TContext> source, TCurrent item)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node.Node, new () { Item=source.Node.Item }, item, 2));
    public static SpanHost<TInitial, TCurrent, AppendN<TInitial, TCurrent, AppendItems<TPreviousItems, TCurrent>, TNode>, TContext> Append<TInitial, TCurrent, TNode, TPreviousItems, TContext>(this SpanHost<TInitial, TCurrent, AppendN<TInitial, TCurrent, TPreviousItems, TNode>, TContext> source, TCurrent item)
        where TNode : struct, IStreamNode<TInitial, TCurrent>
        where TPreviousItems : struct =>
        new(in source.Span, new(ref source.Node.Node, source.Node.Items, item, source.Node.Count+1));

    public static SpanHost<TInitial, TCurrent, TNode, TContext> Reverse<TInitial, TCurrent, TNode, TContext>(this SpanHost<TInitial, TCurrent, Reverse<TInitial, TCurrent, TNode>, TContext> source)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, in source.Node.Node);

    public static SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>, TContext> Select<TInitial, TCurrent, TContext>(this SpanHost<TInitial, TInitial, Root<TInitial>, TContext> source, Func<TInitial, TCurrent> selector) =>
        new(in source.Span, new(selector));
    public static SpanHost<TCurrent, TNext, WhereSelectRoot<TCurrent, TNext>, TContext> Select<TCurrent, TNext, TContext>(this SpanHost<TCurrent, TCurrent, WhereRoot<TCurrent>, TContext> source, Func<TCurrent, TNext> selector) =>
        new(in source.Span, new(source.Node.Predicate, selector));
    public static SpanHost<TInitial, TNext, WhereSelect<TInitial, TCurrent, TNext, TNode>, TContext> Select<TInitial, TCurrent, TNext, TNode, TContext>(this SpanHost<TInitial, TCurrent, Where<TInitial, TCurrent, TNode>, TContext> source, Func<TCurrent, TNext> selector)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node.Node, source.Node.Predicate, selector));

    public static TCurrent[] ToArray<TInitial, TCurrent, TContext>(this SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>, TContext> source)
        => IteratorEx.SelectToArray<TInitial, TCurrent, TContext>(in source.Span, source.Node.Selector);

    public static List<TCurrent> ToList<TCurrent, TContext>(this SpanHost<TCurrent, TCurrent, WhereRoot<TCurrent>, TContext> source)
        => IteratorEx.WhereToList<TCurrent, TContext>(in source.Span, source.Node.Predicate);

    public static SpanHost<TInitial, TInitial, WhereRoot<TInitial>, TContext> Where<TInitial, TContext>(this SpanHost<TInitial, TInitial, Root<TInitial>, TContext> source, Func<TInitial, bool> predicate) =>
        new(in source.Span, new(predicate));
    public static SpanHost<TInitial, TCurrent, SelectWhereRoot<TInitial, TCurrent>, TContext> Where<TInitial, TCurrent, TContext>(this SpanHost<TInitial, TCurrent, SelectRoot<TInitial, TCurrent>, TContext> source, Func<TCurrent, bool> predicate) =>
        new(in source.Span, new(source.Node.Selector, predicate));
    public static SpanHost<TInitial, TNext, SelectWhere<TInitial, TCurrent, TNext, TNode>, TContext> Where<TInitial, TCurrent, TNext, TNode, TContext>(this SpanHost<TInitial, TNext, Select<TInitial, TCurrent, TNext, TNode>, TContext> source, Func<TNext, bool> predicate)
        where TNode : struct, IStreamNode<TInitial, TCurrent> =>
        new(in source.Span, new(ref source.Node.Node, source.Node.Selector, predicate));
}
