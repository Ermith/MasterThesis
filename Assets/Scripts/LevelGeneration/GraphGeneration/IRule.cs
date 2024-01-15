using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRule<T>
{
    public bool IsPossible();
    public void Apply(IEdge<T> edge, IGraph<T> graph, ILock l = null);
}

public abstract class BaseRule : IRule<BaseVertex>
{
    private GraphGenerator _generator;
    public BaseRule(GraphGenerator generator)
    {
        _generator = generator;
    }

    public abstract void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, ILock l = null);

    public abstract bool IsPossible();

    internal void RegisterLock(ILock l, BaseVertex vertex)
    {
        vertex.AddLock(l);
        _generator.RegisterLock(l, vertex);
    }

    internal void RegisterKey(IKey k, BaseVertex vertex)
    {
        vertex.AddKey(k);
        _generator.RegisterKey(k, vertex);
    }
}

public class ExtensionRule : BaseRule
{
    public ExtensionRule(GraphGenerator generator) : base(generator) 
    {
    }

    public override void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, ILock l = null)
    {
        BaseVertex newVertex = new();
        graph.RemoveEdge(edge.From, edge.To);
        graph.AddVertex(newVertex);
        graph.AddEdge(edge.From, newVertex);
        graph.AddEdge(newVertex, edge.To);

        if (l == null) return;
        IKey k = l.GetNewKey();
        RegisterLock(l, edge.From);
        RegisterKey(k, newVertex);
    }

    public override bool IsPossible()
    {
        return true;
    }
}

public class CycleRule : BaseRule
{
    public CycleRule(GraphGenerator generator) : base(generator)
    {
    }

    public override void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, ILock l = null)
    {
        BaseVertex a = new();
        BaseVertex b = new();

        graph.RemoveEdge(edge.From, edge.To);
        graph.AddVertex(a);
        graph.AddVertex(b);

        graph.AddEdge(edge.From, a);
        graph.AddEdge(edge.From, b);
        graph.AddEdge(a, edge.To);
        graph.AddEdge(b, edge.To);

        if (l == null) return;
        IKey k = l.GetNewKey();
        RegisterKey(k, edge.To);
        RegisterLock(l, a);
    }

    public override bool IsPossible()
    {
        return true;
    }
}