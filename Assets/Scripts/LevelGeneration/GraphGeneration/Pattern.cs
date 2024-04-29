using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using URandom = UnityEngine.Random;

public enum DangerType
{
    None,
    SoundTraps,
    DeathTraps,
    SecurityCameras
}

public abstract class Pattern
{
    public DangerType DangerType { get; set; }

    protected (GridEdge, GridEdge) AddExtension(GridEdge edge, GridGraph graph)
    {
        (long midX, long midY, long midZ) = edge.GetMid();
        GridVertex v = graph.AddGridVertex(midX, midY, midZ);
        GridEdge e1 = graph.AddGridEdge(edge.From, v, edge.FromDirection, edge.FromDirection.Opposite());
        GridEdge e2 = graph.AddGridEdge(v, edge.To, edge.ToDirection.Opposite(), edge.ToDirection);

        return (e1, e2);
    }

    private (GridEdge, GridEdge) AddSidePath(GridVertex a, GridVertex b, Directions dir, GridGraph graph, bool? closer = null)
    {
        long minX = Math.Min(a.Position.x, b.Position.x);
        long maxX = Math.Max(a.Position.x, b.Position.x);
        long minY = Math.Min(a.Position.y, b.Position.y);
        long maxY = Math.Max(a.Position.y, b.Position.y);

        long x = 0, y = 0;
        Directions caDir = Directions.None;
        Directions cbDir = Directions.None;

        if (dir.Vertical()) // Assumes a.Position.y == b.Position.y
        {
            if (closer == null)
                x = URandom.value > 0.5f ? a.Position.x : b.Position.x;
            else if (closer.Value)
                x = a.Position.x;
            else
                x = b.Position.x;

            // order by X
            if (a.Position.x > b.Position.x)
                (a, b) = (b, a);

            y = graph.GetNewY(minY, minX, maxX, a.Position.z, dir.North());
            caDir = (x == a.Position.x) ? dir.Opposite() : Directions.West;
            cbDir = (x == b.Position.x) ? dir.Opposite() : Directions.East;
        }

        if (dir.Horizontal()) // Assumes a.Position.x == b.Position.x
        {
            if (closer == null)
                y = URandom.value > 0.5f ? a.Position.y : b.Position.y;
            else if (closer.Value)
                y = a.Position.y;
            else
                y = b.Position.y;

            // Order by Y
            if (a.Position.y > b.Position.y)
                (a, b) = (b, a);

            x = graph.GetNewX(minX, minY, maxY, a.Position.z, dir.East());
            caDir = (y == a.Position.y) ? dir.Opposite() : Directions.South;
            cbDir = (y == b.Position.y) ? dir.Opposite() : Directions.North;
        }

        GridVertex c = graph.AddGridVertex(x, y, a.Position.z);
        GridEdge e1 = graph.AddGridEdge(a, c, dir, caDir);
        GridEdge e2 = graph.AddGridEdge(c, b, cbDir, dir);

        return (e1, e2);
    }

    private (GridEdge, GridEdge) AddCornerPath(GridEdge edge, GridGraph graph)
    {
        GridVertex a = edge.From;
        GridVertex b = edge.To;

        Directions acDir = edge.ToDirection.Opposite();
        Directions bcDir = edge.FromDirection.Opposite();

        // We assume one is horizontal and one is vertical -- no overlap
        Directions edgeDirs = edge.FromDirection | edge.ToDirection;
        bool right = edgeDirs.Contains(Directions.West);
        bool up = edgeDirs.Contains(Directions.South);

        (long midX, long midY, long midZ) = edge.GetMid();
        long newX = graph.GetNewX(midX, edge.MinY, edge.MaxY, edge.FromZ, right);
        long newY = graph.GetNewY(midY, edge.MinX, edge.MaxX, edge.FromZ, up);

        GridEdge mockEdge = new();
        mockEdge.From = a;
        mockEdge.To = b;
        mockEdge.FromDirection = acDir;
        mockEdge.ToDirection = bcDir;

        (long expectedX, long expectedY, long _) = mockEdge.GetMid();


        bool obstructed = newX != expectedX || newY != expectedY;
        Directions caDir = obstructed ? bcDir : acDir.Opposite();
        Directions cbDir = obstructed ? acDir : bcDir.Opposite();

        // Corner Extension
        GridVertex c = graph.AddGridVertex(newX, newY, edge.From.Position.z);
        GridEdge e1 = graph.AddGridEdge(a, c, acDir, caDir);
        GridEdge e2 = graph.AddGridEdge(c, b, cbDir, bcDir);

        return (e1, e2);
    }

    private bool TrySideAddition(GridEdge edge, GridGraph graph, out GridEdge e1, out GridEdge e2, bool? closer = null)
    {
        GridVertex a = edge.From;
        GridVertex b = edge.To;
        var sidePathDir = Directions.None;
        if (edge.FromX == edge.ToX)
        {
            if (!edge.From.Exits.East() && !edge.To.Exits.East())
                sidePathDir = Directions.East;

            else if (!edge.From.Exits.West() && !edge.To.Exits.West())
                sidePathDir = Directions.West;
        }

        if (edge.FromY == edge.ToY)
        {
            if (!edge.From.Exits.North() && !edge.To.Exits.North())
                sidePathDir = Directions.North;

            else if (!edge.From.Exits.South() && !edge.To.Exits.South())
                sidePathDir = Directions.South;
        }

        if (!sidePathDir.None())
        {
            //graph.RemoveGridEdge(edge);
            //AddExtension(edge, graph);
            (e1, e2) = AddSidePath(a, b, sidePathDir, graph, closer);
            return true;
        }

        e1 = e2 = null;
        return false;
    }

    private bool TryCornerAddition(GridEdge edge, GridGraph graph, out GridEdge e1, out GridEdge e2)
    {
        e1 = null;
        e2 = null;

        if (edge.From.Exits.Contains(edge.ToDirection.Opposite()))
            return false;

        if (edge.To.Exits.Contains(edge.FromDirection.Opposite()))
            return false;

        //graph.RemoveGridEdge(edge);
        //AddExtension(edge, graph);
        (e1, e2) = AddCornerPath(edge, graph);
        return true;
    }

    private bool ComplexAddition(GridEdge edge, GridGraph graph, out GridEdge e1, out GridEdge e2, out GridEdge e3, bool? closer = null)
    {
        graph.RemoveGridEdge(edge);
        (GridEdge a, GridEdge b) = AddExtension(edge, graph);

        if (TrySideAddition(a, graph, out e1, out e2, closer)) { e3 = a; return true; }
        if (TrySideAddition(b, graph, out e1, out e2, closer)) { e3 = b; return true; }
        if (TryCornerAddition(a, graph, out e1, out e2)) { e3 = a; return true; }
        if (TryCornerAddition(b, graph, out e1, out e2)) { e3 = b; return true; }

        graph.RemoveGridEdge(a);
        (a, b) = AddExtension(a, graph);
        e3 = b;
        if (TrySideAddition(b, graph, out e1, out e2, closer)) return true;
        if (TryCornerAddition(b, graph, out e1, out e2)) return true;

        return false;
    }

    protected (GridEdge, GridEdge, GridEdge) AddCycle(GridEdge edge, GridGraph graph, bool reversed = false, bool? closer = null)
    {
        GridEdge e1, e2, e3 = edge;
        if (TrySideAddition(edge, graph, out e1, out e2, closer))
        { } else if (TryCornerAddition(edge, graph, out e1, out e2))
        { } else ComplexAddition(edge, graph, out e1, out e2, out e3, closer);

        if (reversed)
        {
            graph.Reverse(e1);
            graph.Reverse(e2);
            var tmp = e1;
            e1 = e2;
            e2 = tmp;
        }

        return (e1, e2, e3);

        throw new Exception("Unable to add cycle");
    }

    protected (GridEdge, GridEdge, GridEdge) AddFloorCycle(GridEdge edge, GridGraph graph, bool toFirst = false, bool reversed = false)
    {
        GridEdge toFork, fromFork;
        Directions dir;
        if (toFirst)
        {
            toFork = AddFork(edge.To, graph, reversed: !reversed);
            if (toFork == null) return (null, null, null);

            dir = reversed ? toFork.FromDirection : toFork.ToDirection;
            fromFork = AddFork(edge.From, graph, dir, reversed);
            if (fromFork == null) return (null, null, null);
        } else
        {
            fromFork = AddFork(edge.From, graph, reversed: reversed);
            if (fromFork == null) return (null, null, null);

            dir = reversed ? fromFork.ToDirection : fromFork.FromDirection;
            toFork = AddFork(edge.To, graph, dir, !reversed);
            if (toFork == null) return (null, null, null);
        }

        var a = reversed ? toFork.To : fromFork.To;
        var b = reversed ? fromFork.From : toFork.From;

        long x = dir.West()
            ? Math.Max(a.Position.x, b.Position.x)
            : Math.Min(a.Position.x, b.Position.x);

        long y = dir.South()
            ? Math.Max(a.Position.y, b.Position.y)
            : Math.Min(a.Position.y, b.Position.y);

        a.Position.x = x;
        a.Position.y = y;
        b.Position.x = x;
        b.Position.y = y;

        var ie = graph.AddInterFloorEdge(a, b);

        return reversed ? (toFork, ie, fromFork) : (toFork, ie, fromFork);
    }

    public GridEdge AddFork(GridVertex v, GridGraph graph, Directions dir = Directions.None, bool reversed = false)
    {
        if (dir.None())
            dir = (~v.Exits).ChooseRandom();

        GridEdge e = null;

        if (dir.Horizontal())
        {
            long x = graph.GetNewX(v.Position.x, v.Position.y, v.Position.y + 1, v.Position.z, dir.East());
            GridVertex c = graph.AddGridVertex(x, v.Position.y, v.Position.z);

            if (reversed)
                e = graph.AddGridEdge(c, v, dir.Opposite(), dir);
            else
                e = graph.AddGridEdge(v, c, dir, dir.Opposite());
        }

        if (dir.Vertical())
        {
            long y = graph.GetNewY(v.Position.y, v.Position.x, v.Position.x + 1, v.Position.z, dir.North());
            GridVertex c = graph.AddGridVertex(v.Position.x, y, v.Position.z);

            if (reversed)
                e = graph.AddGridEdge(c, v, dir.Opposite(), dir);
            else
                e = graph.AddGridEdge(v, c, dir, dir.Opposite());
        }

        return e;
    }

    protected (GridEdge, GridEdge) AddInterFloorExtension(GridEdge edge, GridGraph graph)
    {
        (long midX, long midY, long midZ) = edge.GetMid();
        GridVertex v = graph.AddGridVertex(midX, midY, midZ);
        GridEdge e1 = graph.AddInterFloorEdge(edge.From, v);
        GridEdge e2 = graph.AddInterFloorEdge(v, edge.To);

        return (e1, e2);
    }

    protected ILock GetDangers()
    {
        switch (DangerType)
        {
            case DangerType.SoundTraps:
                return new SoundTrapLock();
            case DangerType.DeathTraps:
                return new DeathTrapLock();
            case DangerType.SecurityCameras:
                return new SecurityCameraLock();
            default:
                return null;
        }
    }

    public abstract void Apply(GridEdge edge, GridGraph graph);
}

public class LockedForkPattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        graph.RemoveGridEdge(edge);
        (GridEdge e1, GridEdge e2) = AddExtension(edge, graph);

        // Lock the main path
        DoorLock @lock = new(e2.FromDirection);
        IKey key = @lock.GetNewKey();
        e2.From.AddLock(@lock);

        var danger = GetDangers();
        var fork = AddFork(e2.From, graph);
        fork.To.AddLock(danger);
        fork.To.AddLock(new EnemyLock());
        fork.To.AddKey(key);
    }
}

public class AlternatePathPattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        (GridEdge ce1, GridEdge ce2, GridEdge ceBase) = AddCycle(edge, graph);
        graph.RemoveGridEdge(ceBase);
        (GridEdge e1, GridEdge e2) = AddExtension(ceBase, graph);

        var danger = GetDangers();
        ce1.To.AddLock(new EnemyLock());
        e1.To.AddLock(danger);
        e1.To.AddLock(new EnemyLock());
    }
}

public class LockedCyclePattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        graph.RemoveGridEdge(edge);
        (GridEdge e1, GridEdge e2) = AddExtension(edge, graph);
        (GridEdge ce1, GridEdge ce2, GridEdge _) = AddCycle(e1, graph, reversed: true, closer: true);
        ce1.To.Hallway = true;

        // Lock the main path
        DoorLock @lock = new(e2.FromDirection);
        IKey key = @lock.GetNewKey();
        e2.From.AddLock(@lock);

        // Extend path back to the main room
        // This is so the key is right next to the original room
        //graph.RemoveGridEdge(ce2);
        //(ce1, ce2) = AddExtension(ce2, graph);
        //ce1.To.Hallway = true;

        // Add 'Valve' back to the main room
        WallOfLightLock @lock2 = new(ce2.ToDirection);
        IKey key2 = @lock2.GetNewKey();
        ce2.From.AddKey(key2);
        ce2.To.AddLock(@lock2);

        // Add Key to the the side path
        ce2.From.AddKey(key);
        ce2.From.AddLock(new EnemyLock());

        if (URandom.value > 0.7f)
            ce2.From.AddKey(new SideObjectiveKey());
    }
}

public class HiddenPathPattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        (GridEdge ce1, GridEdge ce2, GridEdge ceBase) = AddCycle(edge, graph);
        edge = ceBase;
        graph.RemoveGridEdge(edge);
        (GridEdge e1, GridEdge e2) = AddExtension(edge, graph);

        EnemyLock enemyLock = new();

        // Make side path Hidden
        var hidden = URandom.value > 0.5f ? ce1 : ce2;
        HiddenDoorLock fromLock = new(hidden.FromDirection);
        HiddenDoorLock toLock = new(hidden.ToDirection);
        hidden.From.AddLock(fromLock);
        hidden.To.AddLock(toLock);
        hidden.Hidden = true;


        // Make the main path dnageorus
        ILock primaryDanger = GetDangers();
        e1.To.AddLock(primaryDanger);
        e1.To.AddLock(enemyLock);
        ce1.To.AddKey(primaryDanger?.GetNewKey());
        ce1.To.AddKey(enemyLock?.GetNewKey());

        if (URandom.value > 0.7f)
            ce1.To.AddKey(new SideObjectiveKey());
    }
}

public class DoubleLockCyclePattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        graph.RemoveGridEdge(edge);
        (GridEdge e1, GridEdge e2) = AddExtension(edge, graph);
        (GridEdge ce1, GridEdge ce2, GridEdge _) = AddCycle(e1, graph);

        // Lock & Key 'A'
        var doorLock = new DoorLock(ce1.FromDirection);
        var key = doorLock.GetNewKey();
        ce1.From.AddLock(doorLock);
        e1.To.AddKey(key);

        // Lock & Key 'B'
        doorLock = new DoorLock(e2.FromDirection);
        key = doorLock.GetNewKey();
        e2.From.AddLock(doorLock);
        ce2.From.AddKey(key);

        if (URandom.value > 0.7f)
            ce2.From.AddKey(new SideObjectiveKey());

        // Valve
        var wallOfLight = new WallOfLightLock(ce2.FromDirection);
        var powerBox = wallOfLight.GetNewKey();
        ce2.From.AddLock(wallOfLight);
        ce2.From.AddKey(powerBox);

    }
}

public class FloorHiddenPathPattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        GridVertex oldVertex = null;
        bool up = false;
        bool down = false;

        if (!edge.To.Bottom)
        {
            oldVertex = edge.To;
            down = true;
        } else if (!edge.To.Top)
        {
            oldVertex = edge.To;
            up = true;
        } else
        if (!edge.From.Bottom)
        {
            oldVertex = edge.From;
            down = true;
        } else if (!edge.From.Top)
        {
            oldVertex = edge.From;
            up = true;
        }

        GridEdge interFloorEdge;
        GridVertex newVertex;

        // perform addition
        if ((up || down) && URandom.value > 0.5f)
        {
            long newZ = graph.GetNewZ(oldVertex.Position.z, oldVertex.Position.x, oldVertex.Position.y, !down);

            newVertex = graph.AddGridVertex(
            oldVertex.Position.x,
            oldVertex.Position.y,
            newZ);

            var ie = graph.AddInterFloorEdge(oldVertex, newVertex);
            interFloorEdge = AddFloorCycle(ie, graph, toFirst: false, reversed: true).Item2;

            if (interFloorEdge == null)
            {
                graph.RemoveGridEdge(ie);
                (GridEdge ie1, GridEdge ie2) = AddInterFloorExtension(ie, graph);
                oldVertex = ie2.From;
                interFloorEdge = AddFloorCycle(ie2, graph, toFirst: false, reversed: true).Item2;
            }
        } else
        { // Perform extension
            graph.RemoveGridEdge(edge);
            (GridEdge e1, GridEdge e2) = AddInterFloorExtension(edge, graph);
            oldVertex = e2.From;
            newVertex = e2.To;

            if (oldVertex.Position.z < newVertex.Position.z) up = true;
            else down = true;

            interFloorEdge = AddFloorCycle(e2, graph, toFirst: true, reversed: true).Item2;

            if (interFloorEdge == null)
            {
                graph.RemoveGridEdge(e2);
                (GridEdge ie1, GridEdge ie2) = AddInterFloorExtension(e2, graph);
                newVertex = ie1.To;
                interFloorEdge = AddFloorCycle(ie1, graph, toFirst: true, reversed: true).Item2;
            }
        }

        if (down) oldVertex.AddLock(new HiddenDoorLock(down: true));
        else if (up) oldVertex.AddLock(new HiddenDoorLock(up: true));

        ILock primaryDanger = GetDangers();
        ILock enemyLock = new EnemyLock();

        interFloorEdge.From.AddLock(primaryDanger);
        interFloorEdge.From.AddLock(enemyLock);
        interFloorEdge.To.AddLock(primaryDanger);
        interFloorEdge.To.AddLock(enemyLock);

        newVertex.AddKey(enemyLock.GetNewKey());
    }
}

public class FloorLockedCyclePattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        GridVertex oldVertex = null;
        bool up = false;
        bool down = false;

        if (!edge.To.Bottom)
        {
            oldVertex = edge.To;
            down = true;
        } else if (!edge.To.Top)
        {
            oldVertex = edge.To;
            up = true;
        } else
        if (!edge.From.Bottom)
        {
            oldVertex = edge.From;
            down = true;
        } else if (!edge.From.Top)
        {
            oldVertex = edge.From;
            up = true;
        }

        if (down && up)
        {
            if (URandom.value > 0.5f)
                down = false;
            else
                up = false;
        }

        ILock enemyLock = new EnemyLock();
        ILock wallOfLight;
        ILock primaryDanger = GetDangers();

        if ((up || down) && URandom.value > 0.5f)
        {
            long newZ = graph.GetNewZ(oldVertex.Position.z, oldVertex.Position.x, oldVertex.Position.y, !down);

            var newVertex = graph.AddGridVertex(
            oldVertex.Position.x,
            oldVertex.Position.y,
            newZ);

            var ie = graph.AddInterFloorEdge(newVertex, oldVertex);
            (GridEdge ce1, GridEdge ce2, GridEdge ce3) = AddFloorCycle(ie, graph, toFirst: true, reversed: true);

            if (ce1 == null)
            {
                graph.RemoveGridEdge(ie);
                (GridEdge e1, GridEdge e2) = AddInterFloorExtension(ie, graph);
                ie = e1;
                (ce1, ce2, ce3) = AddFloorCycle(ie, graph, toFirst: true, reversed: true);
            }

            wallOfLight = new WallOfLightLock(upExit: down, downExit: up);
            ie.From.AddLock(wallOfLight);
            ie.From.AddKey(wallOfLight.GetNewKey());
            ce3.From.AddKey(enemyLock.GetNewKey());

            ce1.To.AddLock(primaryDanger);
            ce1.To.AddLock(enemyLock);
            ce2.To.AddKey(primaryDanger?.GetNewKey());
            ce2.To.AddKey(enemyLock?.GetNewKey());
            ce2.To.AddKey(new SideObjectiveKey());
        } else
        {
            graph.RemoveGridEdge(edge);
            (GridEdge a, GridEdge b) = AddInterFloorExtension(edge, graph);
            (GridEdge ce1, GridEdge ce2, GridEdge ce3) = AddFloorCycle(a, graph, toFirst: false, reversed: true);

            if (ce1 == null)
            {
                graph.RemoveGridEdge(a);
                (GridEdge e1, GridEdge e2) = AddInterFloorExtension(a, graph);
                (ce1, ce2, ce3) = AddFloorCycle(e2, graph, toFirst: true, reversed: true);
            }

            wallOfLight = new WallOfLightLock(ce3.FromDirection);
            ce3.From.AddLock(wallOfLight);
            ce3.From.AddKey(wallOfLight.GetNewKey());
            ce3.From.AddKey(enemyLock.GetNewKey());
            ce3.From.AddKey(new SideObjectiveKey());

            ce2.From.AddKey(primaryDanger?.GetNewKey());

            ce1.To.AddLock(primaryDanger);
            ce1.To.AddLock(enemyLock);

        }
    }
}

public class FloorLockedForkPattern : Pattern
{
    public override void Apply(GridEdge edge, GridGraph graph)
    {
        GridVertex oldVertex = null;
        bool up = false;
        bool down = false;

        if (!edge.To.Bottom)
        {
            oldVertex = edge.To;
            down = true;
        } else if (!edge.To.Top)
        {
            oldVertex = edge.To;
            up = true;
        } else
        if (!edge.From.Bottom)
        {
            oldVertex = edge.From;
            down = true;
        } else if (!edge.From.Top)
        {
            oldVertex = edge.From;
            up = true;
        }

        if (down && up)
        {
            if (URandom.value > 0.5f)
                down = false;
            else
                up = false;
        }

        ILock enemyLock = new EnemyLock();
        ILock primaryDanger = GetDangers();

        if ((up || down) && URandom.value > 0.5f)
        {
            long newZ = graph.GetNewZ(oldVertex.Position.z, oldVertex.Position.x, oldVertex.Position.y, !down);

            var newVertex = graph.AddGridVertex(
            oldVertex.Position.x,
            oldVertex.Position.y,
            newZ);

            var ie = graph.AddInterFloorEdge(oldVertex, newVertex);
            GridEdge keyFork = AddFork(newVertex, graph);
            GridEdge bonusFork;

            if (keyFork == null)
            {
                graph.RemoveGridEdge(ie);
                (GridEdge e1, GridEdge e2) = AddInterFloorExtension(ie, graph);
                keyFork = AddFork(e2.To, graph);
                bonusFork = AddFork(e2.From, graph);
            } else
            {
                bonusFork = AddFork(oldVertex, graph);
            }

            ILock @lock = new DoorLock(bonusFork.FromDirection);
            bonusFork.From.AddLock(@lock);
            bonusFork.To.AddKey(enemyLock.GetNewKey()); // Bonus?
            bonusFork.To.AddKey(primaryDanger?.GetNewKey());
            bonusFork.To.AddKey(new SideObjectiveKey());
            keyFork.To.AddKey(@lock.GetNewKey());
            keyFork.To.AddLock(primaryDanger);
            keyFork.To.AddLock(enemyLock);
        } else
        {
            graph.RemoveGridEdge(edge);
            (GridEdge e1, GridEdge e2) = AddInterFloorExtension(edge, graph);
            GridEdge bonusFork = null;
            GridEdge keyFork = AddFork(e1.From, graph);
            GridVertex lockedStairway = e1.To;

            if (keyFork == null)
            {
                graph.RemoveGridEdge(e1);
                (e1, e2) = AddInterFloorExtension(e1, graph);
                keyFork = AddFork(e2.From, graph);
                bonusFork = AddFork(e2.To, graph);
                lockedStairway = e2.To;

            } else
            {
                bonusFork = AddFork(e1.To, graph);
            }

            ILock @lock = new DoorLock(down: e1.FromZ < e1.ToZ, up: e1.FromZ > e1.ToZ);
            lockedStairway.AddLock(@lock);

            keyFork.To.AddKey(@lock.GetNewKey());
            keyFork.To.AddLock(primaryDanger);
            keyFork.To.AddLock(enemyLock);

            // Add bonus? 
            bonusFork.To.AddKey(enemyLock.GetNewKey());
            bonusFork.To.AddKey(new SideObjectiveKey());
        }

    }
}