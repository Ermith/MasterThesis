using System;
using System.Collections.Generic;

/// <summary>
/// Representing a graph drawn into a grid of size [MaximumX, MaximumY, MaximumZ].
/// </summary>
/// <typeparam name="T"></typeparam>
public struct GraphDrawing<T>
{
    public Dictionary<T, (int, int, int)> VertexPositions;
    public Dictionary<IEdge<T>, List<(int, int, int)>> EdgePositions;
    public int MaximumX;
    public int MaximumY;
    public int MaximumZ;
    public T StartPosition;
    public T EndPosition;
}

/// <summary>
/// Relies on <see cref="GridGraph"/>. Transforms the topological information into a <see cref="GraphDrawing{T}"/>.
/// </summary>
public class GraphGridDrawer
{
    IGraph<GridVertex> _graph;
    public GraphGridDrawer(IGraph<GridVertex> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Trnasforms <see cref="GridGraph"/> into a <see cref="GraphDrawing{GridVertex}"/>.
    /// </summary>
    /// <param name="startVertex"></param>
    /// <param name="endVertex"></param>
    /// <returns></returns>
    public GraphDrawing<GridVertex> Draw(GridVertex startVertex, GridVertex endVertex)
    {
        Dictionary<GridVertex, (int, int, int)> vertexPositions = new();
        Dictionary<IEdge<GridVertex>, List<(int, int, int)>> edgePositions = new();
        List<long> xx = new();
        List<long> yy = new();
        List<long> zz = new();

        foreach (GridVertex v in _graph.GetVertices())
        {
            if (!xx.Contains(v.Position.x)) xx.Add(v.Position.x);
            if (!yy.Contains(v.Position.y)) yy.Add(v.Position.y);
            if (!zz.Contains(v.Position.z)) zz.Add(v.Position.z);
        }

        xx.Sort();
        yy.Sort();
        zz.Sort();

        int maximumX = xx.Count - 1;
        int maximumY = yy.Count - 1;
        int maximumZ = zz.Count - 1;

        foreach (GridVertex v in _graph.GetVertices())
            vertexPositions.Add(v, (xx.IndexOf(v.Position.x), yy.IndexOf(v.Position.y), zz.IndexOf(v.Position.z)));

        foreach (GridEdge e in _graph.GetEdges())
        {
            (long midX, long midY, long midZ) = e.GetMid();

            var positions = new List<(int, int, int)>();
            
            positions.Add((xx.IndexOf(e.FromX), yy.IndexOf(e.FromY), zz.IndexOf(e.FromZ)));

            if (e.FromX != e.ToX && e.FromY != e.ToY)
                positions.Add((xx.IndexOf(midX), yy.IndexOf(midY), zz.IndexOf(e.FromZ)));

            positions.Add((xx.IndexOf(e.ToX), yy.IndexOf(e.ToY), zz.IndexOf(e.ToZ)));
            
            edgePositions[e] = positions;
        }

        return new GraphDrawing<GridVertex>
        {
            VertexPositions = vertexPositions,
            EdgePositions = edgePositions,
            MaximumX = maximumX,
            MaximumY = maximumY,
            MaximumZ = maximumZ,
            StartPosition = startVertex,
            EndPosition = endVertex,
        };
    }
}