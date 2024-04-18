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
    public bool Hallway = false;
    public bool Top = false;
    public bool Bottom = false;

    public (int x, int y, int z) Position;

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

    public List<(Directions, IKey)> Keys;
    public List<(Directions, ILock)> Locks;

    public int fromX => From.Position.x;
    public int fromY => From.Position.y;
    public int fromZ => From.Position.z;
    public int toX => To.Position.x;
    public int toY => To.Position.y;
    public int toZ => To.Position.z;
    
    public int minX => Mathf.Min(fromX, toX);
    public int minY => Mathf.Min(fromY, toY);
    public int maxX => Mathf.Max(fromX, toX);
    public int maxY => Mathf.Max(fromY, toY);

    public bool Hidden = false;

    public (int x, int y, int z) GetMid()
    {
        int newZ = fromZ + (toZ - fromZ) / 2;

        if (fromX == toX)
            return (fromX, fromY + (toY - fromY) / 2, newZ);

        if (fromY == toY)
            return (fromX + (toX - fromX) / 2, fromY, newZ);

        if (FromDirection.Horizontal())
            return (toX, fromY, newZ);

        if (FromDirection.Vertical())
            return (fromX, toY, newZ);

        // Should not happen
        return (fromX, fromY, newZ);
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

        (int midX, int midY, int midZ) = GetMid();

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

        (int midX, int midY, int midZ) = GetMid();

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
        List<Pattern> patterns = new();
        if (GenerationSettings.PatternDoubleLock) patterns.Add(new DoubleLockCyclePattern());
        if (GenerationSettings.PatternLockedCycle) patterns.Add(new LockedCyclePattern());
        if (GenerationSettings.PatternHiddenShortcut) patterns.Add(new HiddenPathPattern());

        List<Pattern> floorPatterns = new();
        if (GenerationSettings.FloorPatternHiddenShortcut) floorPatterns.Add(new FloorHiddenPathExtensionPattern());
        if (GenerationSettings.FloorPatternLockedExtention) floorPatterns.Add(new FloorLockedExtentionPattern());
        if (GenerationSettings.FloorPatternLockedAddition) floorPatterns.Add(new FloorLockedAdditionPattern());

        List<DangerType> dangerTypes = new();
        if (GenerationSettings.DangerCameras) dangerTypes.Add(DangerType.SecurityCameras);
        if (GenerationSettings.DangerSoundTraps) dangerTypes.Add(DangerType.SoundTraps);
        if (GenerationSettings.DangerDeathTraps) dangerTypes.Add(DangerType.DeathTraps);

        if (GenerationSettings.FloorPatternCount == 0)
        {
            _start = Graph.AddGridVertex(0, 0);
            _end = Graph.AddGridVertex(0, BaseRule.STEP);
            Graph.AddGridEdge(_start, _end, Directions.North, Directions.South);
        } else
        {
            var f1 = Graph.AddGridVertex(0, 0, 0);
            var f2 = Graph.AddGridVertex(0, 0, BaseRule.STEP);

            _start = Graph.AddGridVertex(0, -BaseRule.STEP, 0);
            _end = Graph.AddGridVertex(0, BaseRule.STEP, BaseRule.STEP);

            Graph.AddInterFloorEdge(f1, f2);
            Graph.AddGridEdge(_start, f1, Directions.North, Directions.South);
            Graph.AddGridEdge(f2, _end, Directions.North, Directions.South);
        }

        for (int i = 0; i < GenerationSettings.FloorPatternCount; i++)
        {
            int index = URandom.Range(0, floorPatterns.Count);
            GridEdge e = Graph.GetRandomInterfloorEdge();
            floorPatterns[index].Apply(e, Graph);
        }

        for (int floor = 0; floor < Graph.FloorCount; floor++)
        {
            for (int i = 0; i < GenerationSettings.PatternCount; i++)
            {
                int index = URandom.Range(0, patterns.Count);
                //GridEdge e = Graph.LongestEdge(false);
                GridEdge e = Graph.GetRandomFloorEdge(allowHidden: false);
                patterns[index].Apply(e, Graph);
            }
        }
    }
}
