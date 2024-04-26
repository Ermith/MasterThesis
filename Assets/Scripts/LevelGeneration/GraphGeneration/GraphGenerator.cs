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
    public bool SideObjective = false;

    public (long x, long y, long z) Position;

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
        foreach (ILock l in _locks)
            yield return l;
    }

    public IEnumerable<IKey> GetKeys()
    {
        foreach (IKey k in _keys)
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

    public long fromX => From.Position.x;
    public long fromY => From.Position.y;
    public long fromZ => From.Position.z;
    public long toX => To.Position.x;
    public long toY => To.Position.y;
    public long toZ => To.Position.z;

    public long minX => Math.Min(fromX, toX);
    public long minY => Math.Min(fromY, toY);
    public long minZ => Math.Min(fromZ, toZ);
    public long maxX => Math.Max(fromX, toX);
    public long maxY => Math.Max(fromY, toY);
    public long maxZ => Math.Max(fromZ, toZ);

    public bool Hidden = false;

    public (long x, long y, long z) GetMid()
    {
        long newZ = fromZ + (toZ - fromZ) / 2;

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

    public long? GetHorizontalOffset(long lowerBound, long upperBound, long x)
    {
        float verticalOverlap = Mathf.Min(maxY, upperBound) - Mathf.Max(minY, lowerBound);

        // They are not even overlapping
        if (verticalOverlap < 0)
            return null;

        long offset = 0;

        // Calculate horizontal offset of the vertical segment
        //   | <---> | 

        long vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? minX : maxX;
        offset += vx - x;


        // Calculate horizontal offset of the horizontal segment
        //   | <--->  --
        long hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? minY : maxY;
        long hOffset = minX - x;
        if (hy > lowerBound && hy < upperBound) // lower-upper are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(hOffset)) // The closer offset
                offset = hOffset;

        return offset;
    }

    public long? GetVerticalOffset(long leftBound, long rightBound, long y)
    {
        long horizontalOverlap = Math.Min(maxX, rightBound) - Math.Max(minX, leftBound);

        // They are not even overlapping
        if (horizontalOverlap < 0)
            return null;

        long offset = 0;

        // Calculate vertical offset of the horizontal segment
        // -----
        //   ^
        //   |
        //   v
        // -----
        long hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? minY : maxY;
        offset += hy - y;


        // Calculate vertical offset of the vertical segment
        // -----
        //   ^
        //   |
        //   v
        //   
        //   |
        long vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? minX : maxX;
        long vOffset = minY - y;
        if (vx > leftBound && vx < rightBound) // left-right are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(vOffset)) // The closer offset
                offset = vOffset;

        return offset;
    }

    public (long, long, long)? GetVerticalLine()
    {
        if (fromX == toX)
            return (fromX, minY, maxY);

        (long midX, long midY, long midZ) = GetMid();

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

    public (long, long, long)? GetHorizontalLine()
    {
        if (fromY == toY)
            return (minX, maxX, toY);

        (long midX, long midY, long midZ) = GetMid();

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
        if (GenerationSettings.PatternLockedFork) patterns.Add(new LockedForkPattern());
        if (GenerationSettings.PatternAlternativePath) patterns.Add(new AlternatePathPattern());

        List<Pattern> floorPatterns = new();
        if (GenerationSettings.FloorPatternHiddenShortcut) floorPatterns.Add(new FloorHiddenPathPattern());
        if (GenerationSettings.FloorPatternLockedCycle) floorPatterns.Add(new FloorLockedCyclePattern());
        if (GenerationSettings.FloorPatternLockedFork) floorPatterns.Add(new FloorLockedForkPattern());

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
            int dangerIndex = URandom.Range(0, dangerTypes.Count);
            GridEdge e = Graph.GetRandomInterfloorEdge();
            var pattern = floorPatterns[index];
            if (dangerTypes.Count > 0)
                pattern.DangerType = dangerTypes[dangerIndex];
            pattern.Apply(e, Graph);
        }

        for (long floor = 0; floor < Graph.FloorCount; floor++)
        {
            for (int i = 0; i < GenerationSettings.PatternCount; i++)
            {
                int index = URandom.Range(0, patterns.Count);
                int dangerIndex = URandom.Range(0, dangerTypes.Count);
                //GridEdge e = Graph.LongestEdge(false);
                GridEdge e = Graph.GetRandomFloorEdge(floor, allowHidden: false);
                var pattern = patterns[index];
                if (dangerTypes.Count > 0)
                    pattern.DangerType = dangerTypes[dangerIndex];
                pattern.Apply(e, Graph);
            }
        }
    }
}
