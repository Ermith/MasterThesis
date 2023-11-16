using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URandom = UnityEngine.Random;

public class DFSParams<T>
{
    public int EnterTimeCurrent = 0;
    public int ExitTimeCurrent = 0;
    public Dictionary<T, int> EnterTimes = new();
    public Dictionary<T, int> ExitTimes = new();
    public Dictionary<T, T> Low = new();
    public Dictionary<T, T> Parents = new();
    public List<T> Preorder = new();
}

public interface IEdge<T>
{
    public T From { get; set; }
    public T To { get; set; }

    public T Neighbor(T vertex);
    public bool Is(T from, T to);
    public bool IsUndirected(T from, T to);
}

public class BaseEdge<T> : IEdge<T>
{
    public T From { get; set; }

    public T To { get; set; }

    public bool Is(T from, T to)
    {
        return From.Equals(from) && To.Equals(to);
    }

    public bool IsUndirected(T from, T to)
    {
        return Is(from, to) || Is(to, from);
    }

    public T Neighbor(T vertex)
    {
        if (vertex.Equals(From)) return To;
        if (vertex.Equals(To)) return From;
        return default;
    }
}

public interface IGraph<T>
{
    public int VertexCount { get; }
    public void AddVertex(T vertex);
    public void AddEdge(T from, T to, IEdge<T> edge = null);
    public void RemoveVertex(T vertex);
    public void RemoveEdge(T from, T to);
    public IEnumerable<T> GetNeighbors(T vertex);
    public IEnumerable<T> GetVertices();
    public IEnumerable<(T, T)> GetEdgePairs();
    public IEnumerable<IEdge<T>> GetEdges();
    public IEdge<T> GetEdge(T from, T to);
    public IEdge<T> GetRandomEdge();
    public Dictionary<T, int> STNumbering(T from, T to);
    public DFSParams<T> DepthFirstSearch(T start);
}

abstract class AGraph<T> : IGraph<T>
{
    public abstract int VertexCount { get; }
    public abstract void AddEdge(T from, T to, IEdge<T> edge = null);
    public abstract void AddVertex(T vertex);
    public abstract void RemoveVertex(T vertex);
    public abstract void RemoveEdge(T from, T to);
    public abstract IEnumerable<(T, T)> GetEdgePairs();
    public abstract IEnumerable<IEdge<T>> GetEdges();
    public abstract IEdge<T> GetRandomEdge();
    public abstract IEdge<T> GetEdge(T from, T to);
    public abstract IEnumerable<T> GetNeighbors(T vertex);
    public abstract IEnumerable<T> GetVertices();

    private void DFSInner(T vertex, DFSParams<T> dfsParams)
    {
        dfsParams.Preorder.Add(vertex);
        dfsParams.EnterTimes[vertex] = ++dfsParams.EnterTimeCurrent;
        dfsParams.Low[vertex] = vertex;

        foreach (T neighbor in GetNeighbors(vertex))
        {
            if (dfsParams.EnterTimes[neighbor] == 0)
            {
                DFSInner(neighbor, dfsParams);
                dfsParams.Parents[neighbor] = vertex;

                if (dfsParams.EnterTimes[dfsParams.Low[neighbor]] < dfsParams.EnterTimes[dfsParams.Low[vertex]])
                    dfsParams.Low[vertex] = dfsParams.Low[neighbor];

            } else if (dfsParams.EnterTimes[neighbor] < dfsParams.EnterTimes[dfsParams.Low[vertex]])
            {
                dfsParams.Low[vertex] = neighbor;
            }
        }

        dfsParams.ExitTimes[vertex] = dfsParams.ExitTimeCurrent++;
    }

    public DFSParams<T> DepthFirstSearch(T start)
    {
        DFSParams<T> dfsParams = new();

        foreach (T vertex in GetVertices())
            dfsParams.EnterTimes[vertex] = 0;

        DFSInner(start, dfsParams);

        return dfsParams;
    }

    public Dictionary<T, int> STNumbering(T start, T target)
    {
        //var edge = GetEdge(start, target);
        //if (edge == null) AddEdge(start, target);
        //var p = DepthFirstSearch(target);
        //if (edge == null) RemoveEdge(start, target);

        UndirectedAdjecencyGraph<T> graph = new();
        foreach (T vertex in GetVertices())
            graph.AddVertex(vertex);


        graph.AddEdge(start, target);
        foreach ((T from, T to) in GetEdgePairs())
            graph.AddEdge(from, to);

        var p = graph.DepthFirstSearch(start);

        Dictionary<T, bool> positives = new();
        positives[start] = false;

        Dictionary<T, LinkedListNode<T>> llNodes = new();

        LinkedList<T> verticesLL = new();
        llNodes[start] = verticesLL.AddLast(start);
        llNodes[target] = verticesLL.AddLast(target);

        //bool first = true;
        foreach (T vertex in p.Preorder)
        {
            if (vertex.Equals(start) || vertex.Equals(target)) continue;

            var low = p.Low[vertex];
            T parent = p.Parents[vertex];
            LinkedListNode<T> parentNode = llNodes[parent];

            if (positives[low])

            {
                llNodes[vertex] = verticesLL.AddAfter(parentNode, vertex);
                positives[parent] = false;
            } else
            {
                llNodes[vertex] = verticesLL.AddBefore(parentNode, vertex);
                positives[parent] = true;
            }
        }

        // Now for the actual numbering
        Debug.Log("ST NUMBERING ======================");
        Dictionary<T, int> stNumbering = new();
        int stNumber = 0;
        foreach (T vertex in verticesLL)
        {
            stNumbering[vertex] = stNumber;
            Debug.Log($"{vertex} = {stNumber}");
            stNumber++;
        }

        return stNumbering;
    }
}

class AdjecencyGraph<T> : AGraph<T>
{
    private Dictionary<T, List<T>> _adjecencyList = new();
    private List<IEdge<T>> _edges = new();

    private int _vertexCount = 0;
    public override int VertexCount => _vertexCount;

    public override void AddVertex(T vertex)
    {
        if (_adjecencyList.ContainsKey(vertex))
            return;

        _adjecencyList[vertex] = new List<T>();
        _vertexCount++;
    }

    public override void AddEdge(T from, T to, IEdge<T> edge = null)
    {
        if (!_adjecencyList.ContainsKey(from) || !_adjecencyList.ContainsKey(to))
            return;

        foreach (IEdge<T> e in _edges)
            if (e.Is(from, to))
                return;

        _adjecencyList[from].Add(to);
        if (edge == null) edge = new BaseEdge<T>();
        edge.From = from;
        edge.To = to;
        _edges.Add(edge);
    }

    public override void RemoveVertex(T vertex)
    {
        _edges.RemoveAll((IEdge<T> edge) => edge.From.Equals(vertex) || edge.To.Equals(vertex));
        _adjecencyList.Remove(vertex);
        foreach ((T _, List<T> neighbors) in _adjecencyList)
            neighbors.Remove(vertex);

        _vertexCount--;
    }

    public override void RemoveEdge(T from, T to)
    {
        _edges.RemoveAll((IEdge<T> edge) => edge.Is(from, to));
        _adjecencyList[from].Remove(to);
    }

    public override IEnumerable<T> GetNeighbors(T vertex)
    {
        foreach (T neighbor in _adjecencyList[vertex])
            yield return neighbor;
    }

    public override IEnumerable<T> GetVertices()
    {
        foreach (T vertex in _adjecencyList.Keys)
            yield return vertex;
    }

    public override IEnumerable<(T, T)> GetEdgePairs()
    {
        foreach ((T vertex, List<T> neighbors) in _adjecencyList)
            foreach (T neighbor in neighbors)
                yield return (vertex, neighbor);
    }

    public override IEnumerable<IEdge<T>> GetEdges()
    {
        foreach (IEdge<T> edge in _edges)
            yield return edge;
    }

    public override IEdge<T> GetEdge(T from, T to)
    {
        foreach (IEdge<T> edge in _edges)
            if (edge.Is(from, to))
                return edge;

        return null;
    }
    public override IEdge<T> GetRandomEdge()
    {
        int index = URandom.Range(0, _edges.Count - 1);
        return _edges[index];
    }
}

class UndirectedAdjecencyGraph<T> : AGraph<T>
{
    List<T> _vertices = new();
    List<IEdge<T>> _edges = new();

    private int _vertexCount = 0;
    public override int VertexCount => _vertexCount;

    public override void AddVertex(T vertex)
    {
        _vertices.Add(vertex);
        _vertexCount++;
    }

    public override void AddEdge(T from, T to, IEdge<T> edge = null)
    {
        if (!_vertices.Contains(from) || !_vertices.Contains(to))
            return;

        foreach (IEdge<T> e in _edges)
            if (e.IsUndirected(from, to))
                return;

        if (edge == null) edge = new BaseEdge<T>();
        edge.From = from;
        edge.To = to;
        _edges.Add(edge);
    }

    public override void RemoveVertex(T vertex)
    {
        _edges.RemoveAll((IEdge<T> edge) => edge.From.Equals(vertex) || edge.To.Equals(vertex));
        _vertices.Remove(vertex);
        _vertexCount--;
    }

    public override void RemoveEdge(T from, T to)
    {
        _edges.RemoveAll((IEdge<T> edge) => edge.IsUndirected(from, to));
    }

    public override IEnumerable<(T, T)> GetEdgePairs()
    {
        foreach (IEdge<T> edge in _edges)
            yield return (edge.From, edge.To);
    }

    public override IEnumerable<IEdge<T>> GetEdges()
    {
        foreach (IEdge<T> edge in _edges)
            yield return edge;
    }
    public override IEdge<T> GetEdge(T from, T to)
    {
        foreach (IEdge<T> edge in _edges)
            if (edge.IsUndirected(from, to))
                return edge;

        return null;
    }

    public override IEnumerable<T> GetNeighbors(T vertex)
    {
        foreach (IEdge<T> edge in _edges)
        {
            if (vertex.Equals(edge.From)) yield return edge.To;
            else if (vertex.Equals(edge.To)) yield return edge.From;
        }
    }

    public override IEnumerable<T> GetVertices()
    {
        foreach (T vertex in _vertices)
            yield return vertex;
    }

    public override IEdge<T> GetRandomEdge()
    {
        int index = URandom.Range(0, _edges.Count - 1);
        return _edges[index];
    }
}
