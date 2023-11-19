using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRule<T>
{
    public bool IsPossible();
    public void Apply(IEdge<T> edge, IGraph<T> graph, Lock<T> l = null);
}

public class ExtensionRule : IRule<BaseVertex>
{
    public void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, Lock<BaseVertex> l = null)
    {
        BaseVertex newVertex = new();
        graph.RemoveEdge(edge.From, edge.To);
        graph.AddVertex(newVertex);
        graph.AddEdge(edge.From, newVertex);
        graph.AddEdge(newVertex, edge.To);

        if (l == null) return;
        Key<BaseVertex> k = l.GetNewKey();

        edge.To.Locks.Add(l);
        l.Location = edge.To;

        newVertex.Keys.Add(k);
        k.Location = newVertex;
    }

    public bool IsPossible()
    {
        return true;
    }
}

public class CycleRule : IRule<BaseVertex>
{
    public void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, Lock<BaseVertex> l = null)
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
        Key<BaseVertex> k = l.GetNewKey();
        
        edge.To.Locks.Add(l);
        l.Location = edge.To;

        a.Keys.Add(k);
        k.Location = a;
    }

    public bool IsPossible()
    {
        return true;
    }
}