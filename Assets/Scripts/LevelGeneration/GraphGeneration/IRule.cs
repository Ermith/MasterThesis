using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRule<T>
{
    public bool IsPossible();
    public void Apply(IEdge<T> edge, IGraph<T> graph, Lock l = null);
}

public class ExtensionRule : IRule<BaseVertex>
{
    public void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, Lock l = null)
    {
        BaseVertex newVertex = new();
        graph.RemoveEdge(edge.From, edge.To);
        graph.AddVertex(newVertex);
        graph.AddEdge(edge.From, newVertex);
        graph.AddEdge(newVertex, edge.To);

        edge.To.Locks.Add(l);
        newVertex.Keys.Add(l.GetNewKey());
    }

    public bool IsPossible()
    {
        return true;
    }
}

public class CycleRule : IRule<BaseVertex>
{
    public void Apply(IEdge<BaseVertex> edge, IGraph<BaseVertex> graph, Lock l = null)
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

        edge.To.Locks.Add(l);
        a.Keys.Add(l.GetNewKey());
    }

    public bool IsPossible()
    {
        return true;
    }
}