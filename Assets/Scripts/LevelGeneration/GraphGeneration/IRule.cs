using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;

using URandom = UnityEngine.Random;

public interface IRule
{
    public bool IsPossible();
    public void Apply(GridEdge edge, GridGraph graph, ILock l = null);
}

public abstract class BaseRule : IRule
{
    public const int STEP = 2048;
    private GraphGenerator _generator;
    public BaseRule(GraphGenerator generator)
    {
        _generator = generator;
    }

    public abstract void Apply(GridEdge edge, GridGraph graph, ILock l = null);

    public abstract bool IsPossible();

    internal void RegisterLock(ILock l, GridVertex vertex)
    {
        vertex.AddLock(l);
        _generator.RegisterLock(l, vertex);
    }

    internal void RegisterKey(IKey k, GridVertex vertex)
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

    public override void Apply(GridEdge edge, GridGraph graph, ILock l = null)
    {
        GridVertex newVertex = new();
        GridEdge e1 = new();
        GridEdge e2 = new();
        e1.FromDirection = edge.FromDirection;
        e1.ToDirection = edge.FromDirection.Opposite();
        e2.FromDirection = edge.ToDirection.Opposite();
        e2.ToDirection = edge.ToDirection;
        newVertex.Exits |= edge.FromDirection.Opposite() | edge.ToDirection.Opposite();

        newVertex.Position = edge.GetMid();
        graph.RemoveEdge(edge.From, edge.To);
        graph.AddVertex(newVertex);
        graph.AddEdge(edge.From, newVertex, e1);
        graph.AddEdge(newVertex, edge.To, e2);

        if (l == null) return;
        IKey k = l.GetNewKey();
        RegisterKey(k, edge.From);
        RegisterLock(l, newVertex);
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

    private (GridEdge, GridEdge) AddExtension(GridEdge edge, GridGraph graph)
    {
        (int midX, int midY) = edge.GetMid();
        GridVertex v = graph.AddGridVertex(midX, midY);
        GridEdge e1 = graph.AddGridEdge(edge.From, v, edge.FromDirection, edge.FromDirection.Opposite());
        GridEdge e2 = graph.AddGridEdge(v, edge.To, edge.ToDirection.Opposite(), edge.ToDirection);

        return (e1, e2);
    }

    private GridVertex AddSidePath(GridVertex a, GridVertex b, Directions dir, GridGraph graph)
    {
        int minX = Math.Min(a.Position.x, b.Position.x);
        int maxX = Math.Max(a.Position.x, b.Position.x);
        int minY = Math.Min(a.Position.y, b.Position.y);
        int maxY = Math.Max(a.Position.y, b.Position.y);

        int x = 0, y = 0;
        Directions caDir = Directions.None;
        Directions cbDir = Directions.None;

        if (dir.Vertical()) // Assumes a.Position.y == b.Position.y
        {
            // order by X
            if (a.Position.x > b.Position.x)
                (a, b) = (b, a);

            x = URandom.value > 0.5f ? a.Position.x : b.Position.x; // TODO make random choice
            //x = a.Position.x;
            y = graph.GetNewY(minY, minX, maxX, dir.North());
            caDir = (x == a.Position.x) ? dir.Opposite() : Directions.West;
            cbDir = (x == b.Position.x) ? dir.Opposite() : Directions.East;
        }

        if (dir.Horizontal()) // Assumes a.Position.x == b.Position.x
        {
            // Order by Y
            if (a.Position.y > b.Position.y)
                (a, b) = (b, a);

            x = graph.GetNewX(minX, minY, maxY, dir.East());
            y = URandom.value > 0.5f ? a.Position.y : b.Position.y; // TODO make random choice
            //y = a.Position.y;
            caDir = (y == a.Position.y) ? dir.Opposite() : Directions.South;
            cbDir = (y == b.Position.y) ? dir.Opposite() : Directions.North;
        }

        GridVertex c = graph.AddGridVertex(x, y);
        graph.AddGridEdge(a, c, dir, caDir);
        graph.AddGridEdge(c, b, cbDir, dir);

        return c;
    }

    private GridVertex AddCornerPath(GridEdge edge, GridGraph graph)
    {
        GridVertex a = edge.From;
        GridVertex b = edge.To;

        Directions acDir = edge.ToDirection.Opposite();
        Directions bcDir = edge.FromDirection.Opposite();

        // We assume one is horizontal and one is vertical -- no overlap
        Directions edgeDirs = edge.FromDirection | edge.ToDirection;
        bool right = edgeDirs.Contains(Directions.West);
        bool up = edgeDirs.Contains(Directions.South);

        (int midX, int midY) = edge.GetMid();
        int newX = graph.GetNewX(midX, edge.minY, edge.maxY, right);
        int newY = graph.GetNewY(midY, edge.minX, edge.maxX, up);


        bool obstructed = newX != midX || newY != midY;
        Directions caDir = obstructed ? bcDir : acDir.Opposite();
        Directions cbDir = obstructed ? acDir : bcDir.Opposite();

        // Corner Extension
        GridVertex c = graph.AddGridVertex(newX, newY);
        graph.AddGridEdge(a, c, acDir, caDir);
        graph.AddGridEdge(c, b, cbDir, bcDir);

        return c;
    }

    private bool TrySideAddition(GridEdge edge, GridGraph graph, ILock l)
    {
        GridVertex a = edge.From;
        GridVertex b = edge.To;
        var sidePathDir = Directions.None;
        if (edge.fromX == edge.toX)
        {
            if (!edge.From.Exits.East() && !edge.To.Exits.East())
                sidePathDir = Directions.East;

            else if (!edge.From.Exits.West() && !edge.To.Exits.West())
                sidePathDir = Directions.West;
        }

        //*/
        if (edge.fromY == edge.toY)
        {
            if (!edge.From.Exits.North() && !edge.To.Exits.North())
                sidePathDir = Directions.North;

            else if (!edge.From.Exits.South() && !edge.To.Exits.South())
                sidePathDir = Directions.South;
        }
        //*/

        /*/
        if (edge.fromX == edge.toX)
        {
            // Try them in random order

            if (URandom.value > 0.5f)
            {
                if (!edge.From.Exits.East() && !edge.To.Exits.East())
                    sidePathDir = Directions.East;

                else if (!edge.From.Exits.West() && !edge.To.Exits.West())
                    sidePathDir = Directions.West;
            } else
            {
                if (!edge.From.Exits.West() && !edge.To.Exits.West())
                    sidePathDir = Directions.West;

                else if (!edge.From.Exits.East() && !edge.To.Exits.East())
                    sidePathDir = Directions.East;
            }


        }

        if (edge.fromY == edge.toY)
        {
            // Try them in random order

            if (URandom.value > 0.5f)
            {
                if (!edge.From.Exits.North() && !edge.To.Exits.North())
                    sidePathDir = Directions.North;

                else if (!edge.From.Exits.South() && !edge.To.Exits.South())
                    sidePathDir = Directions.South;
            } else
            {
                if (!edge.From.Exits.South() && !edge.To.Exits.South())
                    sidePathDir = Directions.South;

                else if (!edge.From.Exits.North() && !edge.To.Exits.North())
                    sidePathDir = Directions.North;
            }

        }
        //*/

        if (!sidePathDir.None())
        {
            //graph.RemoveGridEdge(edge);
            //AddExtension(edge, graph);
            AddSidePath(a, b, sidePathDir, graph);
            return true;
        }

        return false;
    }

    private bool TryCornerAddition(GridEdge edge, GridGraph graph, ILock l)
    {
        if (edge.From.Exits.Contains(edge.ToDirection.Opposite()))
            return false;

        if (edge.To.Exits.Contains(edge.FromDirection.Opposite()))
            return false;

        //graph.RemoveGridEdge(edge);
        //AddExtension(edge, graph);
        AddCornerPath(edge, graph);

        return true;
    }

    private void ComplexAddition(GridEdge edge, GridGraph graph, ILock l)
    {
        //*/
        graph.RemoveGridEdge(edge);
        (GridEdge e1, GridEdge e2) = AddExtension(edge, graph);


        // TODO: randomize e1 and e2
        float r = URandom.value;
        r = 0;

        if (r > 0.5f)
        {
            if (TrySideAddition(e1, graph, l)) return;
            if (TrySideAddition(e2, graph, l)) return;
            if (TryCornerAddition(e1, graph, l)) return;
            if (TryCornerAddition(e2, graph, l)) return;
        } else
        {
            if (TrySideAddition(e2, graph, l)) return;
            if (TrySideAddition(e1, graph, l)) return;
            if (TryCornerAddition(e2, graph, l)) return;
            if (TryCornerAddition(e1, graph, l)) return;
        }

        // TODO: Here as well
        r = URandom.value;
        r = 0;
        if (r > 0.5f)
        {
            graph.RemoveGridEdge(e1);
            (e1, e2) = AddExtension(e1, graph);
            if (TrySideAddition(e2, graph, l)) return;
            if (TryCornerAddition(e2, graph, l)) return;
        } else
        {
            graph.RemoveGridEdge(e2);
            (e1, e2) = AddExtension(e2, graph);
            if (TrySideAddition(e1, graph, l)) return;
            if (TryCornerAddition(e1, graph, l)) return;
        }


        // It should not come to this
        throw new Exception("Unable to add cycle addition");
    }

    public override void Apply(GridEdge edge, GridGraph graph, ILock l = null)
    {
        if (TrySideAddition(edge, graph, l)) return;
        if (TryCornerAddition(edge, graph, l)) return;
        ComplexAddition(edge, graph, l);
    }

    public override bool IsPossible()
    {
        return true;
    }
}

public class AdditionRule : BaseRule
{
    public AdditionRule(GraphGenerator generator) : base(generator)
    {
    }

    public override void Apply(GridEdge edge, GridGraph graph, ILock l = null)
    {
        GridVertex v = edge.From;
        Directions dir = (~edge.From.Exits).ChooseRandom();

        if (dir.Horizontal())
        {
            int x = graph.GetNewX(edge.fromX, edge.fromY, edge.fromY + 1, dir.East());
            GridVertex c = graph.AddGridVertex(x, edge.fromY);
            graph.AddGridEdge(edge.From, c, dir, dir.Opposite());
        }

        if (dir.Vertical())
        {
            int y = graph.GetNewY(edge.fromY, edge.fromX, edge.fromX + 1, dir.North());
            GridVertex c = graph.AddGridVertex(edge.fromX, y);
            graph.AddGridEdge(edge.From, c, dir, dir.Opposite());
        }
    }

    public override bool IsPossible()
    {
        return true;
    }
}