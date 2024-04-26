using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

public class GridGraph : AdjecencyGraph<GridVertex>
{
    public const long STEP = 1 << 57;

    public Dictionary<long, List<GridVertex>> Floors = new();
    public Dictionary<long, List<GridEdge>> FloorEdges = new();
    public List<GridEdge> InterFloorEdges = new();

    public int FloorCount => Floors.Keys.Count;

    public GridEdge AddGridEdge(GridVertex from, GridVertex to, Directions fromExit, Directions toExit, GridEdge edge = null)
    {
        if (from.Exits.Contains(fromExit) || to.Exits.Contains(toExit))
            return null;

        edge ??= new();
        edge.FromDirection = fromExit;
        edge.ToDirection = toExit;
        from.Exits |= fromExit;
        to.Exits |= toExit;
        AddEdge(from, to, edge);

        if (from.Position.z != to.Position.z)
            InterFloorEdges.Add(edge);
        else
        {
            if (!FloorEdges.ContainsKey(from.Position.z))
                FloorEdges[from.Position.z] = new List<GridEdge>();

            FloorEdges[from.Position.z].Add(edge);
        }

        return edge;
    }

    public GridEdge AddInterFloorEdge(GridVertex from, GridVertex to, GridEdge edge = null)
    {
        edge ??= new();
        AddEdge(from, to, edge);

        InterFloorEdges.Add(edge);
        var top = from.Position.z > to.Position.z ? from : to;
        var bot = from.Position.z < to.Position.z ? from : to;
        top.Bottom = true;
        bot.Top = true;

        return edge;
    }

    public GridVertex AddGridVertex(long x, long y, long z = 0)
    {
        GridVertex vertex = new GridVertex();
        vertex.Position = (x, y, z);
        AddVertex(vertex);

        if (!Floors.ContainsKey(z))
            Floors[z] = new List<GridVertex>();

        Floors[z].Add(vertex);

        return vertex;
    }

    public long GetNewX(long oldX, long minY, long maxY, long z, bool right)
    {
        if (right)
        {
            long minOffset = STEP * 2;
            if (FloorEdges.ContainsKey(z))
                foreach (GridEdge e in FloorEdges[z])
                {
                    long? offset = e.GetHorizontalOffset(minY, maxY, oldX);
                    if (offset == null || offset <= 0) continue;
                    minOffset = Math.Min(minOffset, offset.Value);
                }

            foreach (GridEdge e in InterFloorEdges)
            {
                if (e.minZ > z || e.maxZ < z)
                    continue;

                long? offset = e.GetHorizontalOffset(minY, maxY, oldX);
                if (offset == null || offset <= 0) continue;
                minOffset = Math.Min(minOffset, offset.Value);
            }

            return oldX + minOffset / 2;
        }

        long maxOffset = -STEP * 2;
        if (FloorEdges.ContainsKey(z))
            foreach (GridEdge e in FloorEdges[z])
            {
                long? offset = e.GetHorizontalOffset(minY, maxY, oldX);
                if (offset == null || offset >= 0) continue;
                maxOffset = Math.Max(maxOffset, offset.Value);
            }

        foreach (GridEdge e in InterFloorEdges)
        {
            if (e.minZ > z || e.maxZ < z)
                continue;

            long? offset = e.GetHorizontalOffset(minY, maxY, oldX);
            if (offset == null || offset >= 0) continue;
            maxOffset = Math.Max(maxOffset, offset.Value);
        }

        return oldX + maxOffset / 2;
    }

    public long GetNewY(long oldY, long minX, long maxX, long z, bool fwd)
    {
        if (fwd)
        {
            long minOffset = STEP * 2;
            if (FloorEdges.ContainsKey(z))
                foreach (GridEdge e in FloorEdges[z])
                {
                    long? offset = e.GetVerticalOffset(minX, maxX, oldY);
                    if (offset == null || offset <= 0) continue;
                    minOffset = Math.Min(minOffset, offset.Value);
                }

            foreach (GridEdge e in InterFloorEdges)
            {
                if (e.minZ > z || e.maxZ < z)
                    continue;

                long? offset = e.GetVerticalOffset(minX, maxX, oldY);
                if (offset == null || offset <= 0) continue;
                minOffset = Math.Min(minOffset, offset.Value);
            }

            return oldY + minOffset / 2;
        }

        long maxOffset = -STEP * 2;
        if (FloorEdges.ContainsKey(z))
            foreach (GridEdge e in FloorEdges[z])
            {
                long? offset = e.GetVerticalOffset(minX, maxX, oldY);
                if (offset == null || offset >= 0) continue;
                maxOffset = Math.Max(maxOffset, offset.Value);
            }

        foreach (GridEdge e in InterFloorEdges)
        {
            if (e.minZ > z || e.maxZ < z)
                continue;

            long? offset = e.GetVerticalOffset(minX, maxX, oldY);
            if (offset == null || offset >= 0) continue;
            maxOffset = Math.Max(maxOffset, offset.Value);
        }

        return oldY + maxOffset / 2;
    }

    public long GetNewZ(long oldZ, long x, long y, bool up)
    {
        long closest;

        if (up)
        {
            closest = oldZ + STEP;
            foreach (long floor in FloorEdges.Keys)
                if (floor > oldZ && floor < closest)
                    closest = floor;

        } else
        {
            closest = oldZ - STEP;
            foreach (long floor in FloorEdges.Keys)
                if (floor < oldZ && floor > closest)
                    closest = floor;
        }

        if (!FloorEdges.ContainsKey(closest))
            return closest;

        var edges = FloorEdges[closest];
        foreach (var edge in edges)
        {
            var horizontal = edge.GetHorizontalLine();
            if (horizontal != null)
            {
                (long minX, long maxX, long ey) = horizontal.Value;
                if (ey == y && minX <= x && maxX >= x)
                    return oldZ + (closest - oldZ) / 2;
            }

            var vertical = edge.GetVerticalLine();
            if (vertical != null)
            {
                (long ex, long minY, long maxY) = vertical.Value;
                if (ex == x && minY <= x && maxY >= x)
                    return oldZ + (closest - oldZ) / 2;
            }
        }

        return closest;
    }

    public void RemoveGridEdge(GridEdge e)
    {
        e.From.Exits = e.From.Exits.Without(e.FromDirection);
        e.To.Exits = e.To.Exits.Without(e.ToDirection);
        foreach (var edges in FloorEdges.Values)
        {
            edges.Remove(e);
        }
        RemoveEdge(e.From, e.To);
    }

    public GridEdge LongestEdge(bool allowHidden = true)
    {
        GridEdge longest = null;
        long length = 0;

        foreach (GridEdge e in GetEdges().Cast<GridEdge>())
        {
            if (e.Hidden && !allowHidden)
                continue;

            long l = e.maxX - e.minX + e.maxY - e.minY;
            if (l > length)
            {
                length = l;
                longest = e;
            }
        }

        return longest;
    }

    public GridEdge GetRandomFloorEdge(long i = -1, bool allowHidden = true)
    {
        var keys = FloorEdges.Keys.ToArray();

        if (i == -1) i = URandom.Range(0, keys.Length);
        var edges = FloorEdges[keys[i]];
        List<GridEdge> filteredEdges = new();
        foreach (var edge in edges)
        {
            if (edge.Hidden && !allowHidden)
                continue;

            filteredEdges.Add(edge);
        }


        int index = URandom.Range(0, filteredEdges.Count);
        return filteredEdges[index];
    }

    public GridEdge GetRandomInterfloorEdge()
    {
        int index = URandom.Range(0, InterFloorEdges.Count);
        return InterFloorEdges[index];
    }

    public void Reverse(GridEdge e)
    {
        RemoveGridEdge(e);
        AddGridEdge(e.To, e.From, e.ToDirection, e.FromDirection, e);
    }
}