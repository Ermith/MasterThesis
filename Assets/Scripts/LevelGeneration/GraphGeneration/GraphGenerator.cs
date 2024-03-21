using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using URandom = UnityEngine.Random;

public class GridVertex
{
    static char c = 'A';
    char _c;

    private List<ILock> _locks = new();
    private List<IKey> _keys = new();

    public Directions Exits;

    public (int x, int y) Position;

    public void AddLock(ILock l)
    {
        _locks.Add(l);
    }

    public void AddKey(IKey k)
    {
        _keys.Add(k);
    }

    public IEnumerable<ILock> GetLocks()
    {
        foreach(ILock l in _locks)
            yield return l;
    }

    public IEnumerable<IKey> GetKeys()
    {
        foreach(IKey k in _keys)
            yield return k;
    }

    public GridVertex()
    {
        _c = c++;
    }

    public override string ToString()
    {
        return $"{_c}";
    }
}

public class GridEdge : BaseEdge<GridVertex>
{
    public Directions FromDirection;
    public Directions ToDirection;

    public int fromX => From.Position.x;
    public int fromY => From.Position.y;
    public int toX => To.Position.x;
    public int toY => To.Position.y;
    
    public int minX => Mathf.Min(fromX, toX);
    public int minY => Mathf.Min(fromY, toY);
    public int maxX => Mathf.Max(fromX, toX);
    public int maxY => Mathf.Max(fromY, toY);

    public (int x, int y) GetMid()
    {
        if (fromX == toX)
            return (fromX, fromY + (toY - fromY) / 2);

        if (fromY == toY)
            return (fromX + (toX - fromX) / 2, fromY);

        if (FromDirection.Horizontal())
            return (toX, fromY);

        if (FromDirection.Vertical())
            return (fromX, toY);

        // Should not happen
        return (fromX, fromY);
    }

    public int? GetHorizontalOffset(int lowerBound, int upperBound, int x)
    {
        float verticalOverlap = Mathf.Min(maxY, upperBound) - Mathf.Max(minY, lowerBound);

        // They are not even overlapping
        if (verticalOverlap <= 0)
            return null;

        int offset = 0;

        // Calculate horizontal offset of the vertical segment
        //   | <---> | 

        int vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? minX : maxX;
        offset += vx - x;


        // Calculate horizontal offset of the horizontal segment
        //   | <--->  --
        int hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? minY : maxY;
        int hOffset = minX - x;
        if (hy > lowerBound && hy < upperBound) // lower-upper are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(hOffset)) // The closer offset
                offset = hOffset;

        return offset;
    }

    public int? GetVerticalOffset(int leftBound, int rightBound, int y)
    {
        int horizontalOverlap = Math.Min(maxX, rightBound) - Math.Max(minX, leftBound);

        // They are not even overlapping
        if (horizontalOverlap <= 0)
            return null;

        int offset = 0;

        // Calculate vertical offset of the horizontal segment
        // -----
        //   ^
        //   |
        //   v
        // -----
        int hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? minY : maxY;
        offset += hy - y;


        // Calculate vertical offset of the vertical segment
        // -----
        //   ^
        //   |
        //   v
        //   
        //   |
        int vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? minX : maxX;
        int vOffset = minY - y;
        if (vx > leftBound && vx < rightBound) // left-right are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(vOffset)) // The closer offset
                offset = vOffset;

        return offset;
    }

    public (int, int, int)? GetVerticalLine()
    {
        if (fromX == toX)
            return (fromX, minY, maxY);

        (int midX, int midY) = GetMid();

        if (fromX == midX)
            return (
                fromX,
                Math.Min(fromY, midY),
                Math.Max(fromY, midY));

        if (midX == toX)
            return (midX,
                Math.Min(midY, toY),
                Math.Max(midY, toY));

        return null;
    }

    public (int, int, int)? GetHorizontalLine()
    {
        if (fromY == toY)
            return (minX, maxX, toY);

        (int midX, int midY) = GetMid();

        if (fromY == midY)
            return (
                Math.Min(fromX, midX),
                Math.Max(fromX, midX),
                midY);

        if (midY == toY)
            return (
                Math.Min(midX, toX),
                Math.Max(midX, toX),
                toY);

        return null;
    }
}

public class GraphGenerator
{
    public GridGraph Graph { get; private set; }
    public Dictionary<ILock, GridVertex> LockMapping = new();
    public Dictionary<IKey, GridVertex> KeyMapping = new();
    private GridVertex _start;
    private GridVertex _end;

    public GraphGenerator(GridGraph graph)
    {
        Graph = graph;
    }

    public void RegisterLock(ILock l, GridVertex vertex) => LockMapping[l] = vertex;
    public void RegisterKey(IKey k, GridVertex vertex) => KeyMapping[k] = vertex;

    public GridVertex GetLockVertex(ILock l) => LockMapping[l];
    public GridVertex GetKeyVertex(IKey k) => KeyMapping[k];

    public GridVertex GetStartVertex() => _start;
    public GridVertex GetEndVertex() => _end;

    public void Generate()
    {
        _start = Graph.AddGridVertex(0, 0);
        _end = Graph.AddGridVertex(BaseRule.STEP, BaseRule.STEP);
        GridEdge e = Graph.AddGridEdge(_start, _end, Directions.North, Directions.West);

        //GridVertex c = Graph.AddGridVertex(BaseRule.STEP, 0);
        //GridVertex d = Graph.AddGridVertex(2*BaseRule.STEP, 0);
        //Graph.AddGridEdge(c, d, Directions.East, Directions.West);

        CycleRule cycleRule = new(this);
        ExtensionRule extensionRule = new(this);
        AdditionRule additionRule = new(this);
        Pattern p = new TestPattern();

        //extensionRule.Apply(Graph.GetEdge(_start, _end) as GridEdge, Graph, new DoorLock());
        //cycleRule.Apply(e, Graph);
        p.Apply(e, Graph);
        //p.Apply(Graph.LongestEdge(), Graph);
        

        for (int i = 0; i < 0; i++)
        {
            GridEdge edge = Graph.GetRandomEdge() as GridEdge;
            edge = Graph.LongestEdge();
            cycleRule.Apply(edge, Graph);

            edge = Graph.GetRandomEdge() as GridEdge;
            edge = Graph.LongestEdge();
            extensionRule.Apply(edge, Graph);
        }

        for (int i = 0; i < 0; i++)
        {
            GridEdge edge = Graph.GetRandomEdge() as GridEdge;
            additionRule.Apply(edge, Graph);
        }

        /*/
        for (int i = 0; i < 0; i++)
        {
            ILock @lock = URandom.value > 0.5f ? new DoorLock() : new DoorLock();
            GridEdge edge;
            do
            {
                edge = Graph.GetRandomEdge() as GridEdge;
            } while (edge.From == _end || edge.To == _start || edge.From == _start);
        
            if (URandom.value > 0.5f)
                cycleRule.Apply(edge, Graph, @lock);
            else
                extensionRule.Apply(edge, Graph, @lock);
        }
        //*/
        
        /*/
        BaseVertex A = new();
        BaseVertex B = new();
        BaseVertex C = new();
        BaseVertex D = new();
        _start = A;
        _end = D;

        Graph.AddVertex(A);
        Graph.AddVertex(B);
        Graph.AddVertex(C);
        Graph.AddVertex(D);
        Graph.AddEdge(A, B);
        Graph.AddEdge(A, C);
        Graph.AddEdge(B, D);
        Graph.AddEdge(C, D);

        CycleRule cycleRule = new(this);
        ExtensionRule extensionRule = new(this);

        int count = 1;
        for (int i = 0; i < count; i++)
        {
            cycleRule.Apply(Graph.GetRandomEdge(), Graph, new DoorLock());
            extensionRule.Apply(Graph.GetRandomEdge(), Graph, new SecurityCameraLock());
        }

        //*/
        //foreach (var vertex in Graph.GetVertices())
        //    if (URandom.value > 0.5f)
        //        vertex.AddLock(new EnemyLock());

        // Graph from the paper
        //PredefinedGraph();
    }

    // For Debugging
    private void PredefinedGraph()
    {
        int count = 8;
        var nodes = new GridVertex[count];
        for (int i = 0; i < count; i++)
        {
            GridVertex vertex = new();
            nodes[i] = vertex;
            Graph.AddVertex(vertex);
        }

        Graph.AddEdge(nodes[0], nodes[1]);
        Graph.AddEdge(nodes[0], nodes[3]);
        Graph.AddEdge(nodes[0], nodes[6]);
        Graph.AddEdge(nodes[1], nodes[3]);
        Graph.AddEdge(nodes[1], nodes[5]);
        Graph.AddEdge(nodes[1], nodes[2]);
        Graph.AddEdge(nodes[2], nodes[5]);
        Graph.AddEdge(nodes[2], nodes[7]);
        Graph.AddEdge(nodes[3], nodes[6]);
        Graph.AddEdge(nodes[3], nodes[4]);
        Graph.AddEdge(nodes[4], nodes[6]);
        Graph.AddEdge(nodes[4], nodes[7]);
        Graph.AddEdge(nodes[4], nodes[5]);
        Graph.AddEdge(nodes[6], nodes[7]);
    }
}
