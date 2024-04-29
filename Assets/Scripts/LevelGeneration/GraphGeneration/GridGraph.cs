using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using URandom = UnityEngine.Random;

/// <summary>
/// Vertex containing grid topological information.
/// Also contains information to be transformed into <see cref="ASuperTile"/>.
/// <see cref="ILock"/> and <see cref="IKey"/> objects can be attatched to this vertex.
/// </summary>
public class GridVertex
{
    static char c = 'A';
    char _c;

    private List<ILock> _locks = new();
    private List<IKey> _keys = new();

    /// <summary>
    /// Which sides have a doorway to exit.
    /// </summary>
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
        // For debug purpouses.
        return $"{_c}";
    }
}

/// <summary>
/// Edge connecting two of <see cref="GridVertex"/>.
/// </summary>
public class GridEdge : BaseEdge<GridVertex>
{
    public Directions FromDirection;
    public Directions ToDirection;

    public List<(Directions, IKey)> Keys;
    public List<(Directions, ILock)> Locks;

    public long FromX => From.Position.x;
    public long FromY => From.Position.y;
    public long FromZ => From.Position.z;
    public long ToX => To.Position.x;
    public long ToY => To.Position.y;
    public long ToZ => To.Position.z;

    public long MinX => Math.Min(FromX, ToX);
    public long MinY => Math.Min(FromY, ToY);
    public long MinZ => Math.Min(FromZ, ToZ);
    public long MaxX => Math.Max(FromX, ToX);
    public long MaxY => Math.Max(FromY, ToY);
    public long MaxZ => Math.Max(FromZ, ToZ);

    public bool Hidden = false;

    public (long x, long y, long z) GetMid()
    {
        long newZ = FromZ + (ToZ - FromZ) / 2;

        // Is horizontal
        if (FromX == ToX)
            return (FromX, FromY + (ToY - FromY) / 2, newZ);

        // Is vertical
        if (FromY == ToY)
            return (FromX + (ToX - FromX) / 2, FromY, newZ);

        // Is corner edge, starting from horizontal.
        if (FromDirection.Horizontal())
            return (ToX, FromY, newZ);

        // Is corner edge, startin from vertical
        if (FromDirection.Vertical())
            return (FromX, ToY, newZ);

        // Should not happen
        return (FromX, FromY, newZ);
    }

    /// <summary>
    /// Gets a new horizontal offset for addition of a new vertex. Calculates based on the closest distance from this edge.
    /// </summary>
    /// <param name="lowerBound"></param>
    /// <param name="upperBound"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public long? GetHorizontalOffset(long lowerBound, long upperBound, long x)
    {
        float verticalOverlap = Mathf.Min(MaxY, upperBound) - Mathf.Max(MinY, lowerBound);

        // They are not even overlapping
        if (verticalOverlap < 0)
            return null;

        long offset = 0;

        // Calculate horizontal offset of the vertical segment
        //   | <---> | 

        long vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? MinX : MaxX;
        offset += vx - x;


        // Calculate horizontal offset of the horizontal segment
        //   | <--->  --
        long hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? MinY : MaxY;
        long hOffset = MinX - x;
        if (hy > lowerBound && hy < upperBound) // lower-upper are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(hOffset)) // The closer offset
                offset = hOffset;

        return offset;
    }

    /// <summary>
    /// Gets a new vertical offset for addition of a new vertex. Calculates based on the closest distance from this edge.
    /// </summary>
    /// <param name="lowerBound"></param>
    /// <param name="upperBound"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public long? GetVerticalOffset(long leftBound, long rightBound, long y)
    {
        long horizontalOverlap = Math.Min(MaxX, rightBound) - Math.Max(MinX, leftBound);

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
        long hy = (FromDirection == Directions.South || ToDirection == Directions.South) ? MinY : MaxY;
        offset += hy - y;


        // Calculate vertical offset of the vertical segment
        // -----
        //   ^
        //   |
        //   v
        //   
        //   |
        long vx = (FromDirection == Directions.West || ToDirection == Directions.West) ? MinX : MaxX;
        long vOffset = MinY - y;
        if (vx > leftBound && vx < rightBound) // left-right are exclusive (not inclusive)
            if (Math.Abs(offset) > Math.Abs(vOffset)) // The closer offset
                offset = vOffset;

        return offset;
    }

    /// <summary>
    /// Gets vertical part of the edge, even if it is a corner edge.
    /// </summary>
    /// <returns>X, MinY, MaxY</returns>
    public (long, long, long)? GetVerticalLine()
    {
        if (FromX == ToX)
            return (FromX, MinY, MaxY);

        (long midX, long midY, long midZ) = GetMid();

        if (FromX == midX)
            return (
                FromX,
                Math.Min(FromY, midY),
                Math.Max(FromY, midY));

        if (midX == ToX)
            return (midX,
                Math.Min(midY, ToY),
                Math.Max(midY, ToY));

        return null;
    }

    /// <summary>
    /// Gets horizontal part of the edge, even if it is a corner edge.
    /// </summary>
    /// <returns>MinX, MaxX, Y</returns>
    public (long, long, long)? GetHorizontalLine()
    {
        if (FromY == ToY)
            return (MinX, MaxX, ToY);

        (long midX, long midY, long midZ) = GetMid();

        if (FromY == midY)
            return (
                Math.Min(FromX, midX),
                Math.Max(FromX, midX),
                midY);

        if (midY == ToY)
            return (
                Math.Min(midX, ToX),
                Math.Max(midX, ToX),
                ToY);

        return null;
    }
}

/// <summary>
/// Directed Adjecency Graph that contains topological information for drasing this graph into a grid.
/// Contains special functions to maintain this topological information.
/// </summary>
public class GridGraph : AdjecencyGraph<GridVertex>
{
    /// <summary>
    /// Base offset for addition of a new vertex.
    /// </summary>
    public const long STEP = 1 << 57;

    /// <summary>
    /// Vertices by floor.
    /// </summary>
    public Dictionary<long, List<GridVertex>> Floors = new();
    /// <summary>
    /// Edges by floor.
    /// </summary>
    public Dictionary<long, List<GridEdge>> FloorEdges = new();
    /// <summary>
    /// Edges connecting two floors.
    /// </summary>
    public List<GridEdge> InterFloorEdges = new();

    public int FloorCount => Floors.Keys.Count;

    /// <summary>
    /// Adds an edge to the graph and attatches it to the exits of connected vertices.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="fromExit"></param>
    /// <param name="toExit"></param>
    /// <param name="edge"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds an edge that connects two floors. Attatches itself to the top and the bottom exit of the given vertices.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="edge"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates and adds a grid vertex.
    /// </summary>
    /// <param name="x">Width</param>
    /// <param name="y">Length</param>
    /// <param name="z">Floor</param>
    /// <returns>Created GridVertex.</returns>
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

    /// <summary>
    /// Gets a new X coordinate for adding a new vertex in horizontal direction.
    /// </summary>
    /// <param name="oldX"></param>
    /// <param name="minY">Range that can obscure this addition.</param>
    /// <param name="maxY">Range that can obscure this addition.</param>
    /// <param name="z">Floor.</param>
    /// <param name="right"></param>
    /// <returns></returns>
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
                if (e.MinZ > z || e.MaxZ < z)
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
            if (e.MinZ > z || e.MaxZ < z)
                continue;

            long? offset = e.GetHorizontalOffset(minY, maxY, oldX);
            if (offset == null || offset >= 0) continue;
            maxOffset = Math.Max(maxOffset, offset.Value);
        }

        return oldX + maxOffset / 2;
    }


    /// <summary>
    /// Gets a new Y coordinate for adding a new vertex in vertical direction.
    /// </summary>
    /// <param name="oldY"></param>
    /// <param name="minX">Range that can obscure this addition.</param>
    /// <param name="maxX">Range that can obscure this addition.</param>
    /// <param name="z">Floor.</param>
    /// <param name="fwd">Forwards or backwards.</param>
    /// <returns></returns>
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
                if (e.MinZ > z || e.MaxZ < z)
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
            if (e.MinZ > z || e.MaxZ < z)
                continue;

            long? offset = e.GetVerticalOffset(minX, maxX, oldY);
            if (offset == null || offset >= 0) continue;
            maxOffset = Math.Max(maxOffset, offset.Value);
        }

        return oldY + maxOffset / 2;
    }

    /// <summary>
    /// Gets a new Y coordinate for adding a new vertex and a new floor..
    /// </summary>
    /// <param name="oldZ"></param>
    /// <param name="x">Range that can obscure this addition.</param>
    /// <param name="y">Range that can obscure this addition.</param>
    /// <param name="up">Forwards or backwards.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Removes the edge and detatches itself from the end vertices exits.
    /// </summary>
    /// <param name="e"></param>
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

    public GridEdge GetLongestEdge(bool allowHidden = true)
    {
        GridEdge longest = null;
        long length = 0;

        foreach (GridEdge e in GetEdges().Cast<GridEdge>())
        {
            if (e.Hidden && !allowHidden)
                continue;

            long l = e.MaxX - e.MinX + e.MaxY - e.MinY;
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

    /// <summary>
    /// Edge connecting two floors.
    /// </summary>
    /// <returns></returns>
    public GridEdge GetRandomInterfloorEdge()
    {
        int index = URandom.Range(0, InterFloorEdges.Count);
        return InterFloorEdges[index];
    }

    /// <summary>
    /// Switches To and From vertices in the edge.
    /// </summary>
    /// <param name="e"></param>
    public void Reverse(GridEdge e)
    {
        RemoveGridEdge(e);
        AddGridEdge(e.To, e.From, e.ToDirection, e.FromDirection, e);
    }
}