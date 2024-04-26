using System;
using System.Collections.Generic;

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

public class GraphGridDrawer
{
    IGraph<GridVertex> _graph;
    public GraphGridDrawer(IGraph<GridVertex> graph)
    {
        _graph = graph;
    }

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
            
            positions.Add((xx.IndexOf(e.fromX), yy.IndexOf(e.fromY), zz.IndexOf(e.fromZ)));

            if (e.fromX != e.toX && e.fromY != e.toY)
                positions.Add((xx.IndexOf(midX), yy.IndexOf(midY), zz.IndexOf(e.fromZ)));

            positions.Add((xx.IndexOf(e.toX), yy.IndexOf(e.toY), zz.IndexOf(e.toZ)));
            

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