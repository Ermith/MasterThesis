using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using URandom = UnityEngine.Random;

/// <summary>
/// World based directions North, West, South, East. Can contain multiple directions at once or none at all.
/// </summary>
[Flags]
public enum Directions
{
    None = 0b0,
    North = 0b1,
    South = 0b10,
    East = 0b100,
    West = 0b1000,
}

// Useful direction for manipulating Directions.
public static class DirectionsExtensions
{
    /// <summary>
    /// Returns true if directions are empty.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool None(this Directions directions) => directions == Directions.None;

    /// <summary>
    /// Returns returns true if directions contain North.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool North(this Directions directions) => (directions & Directions.North) != Directions.None;

    /// <summary>
    /// Returns returns true if directions contain South.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool South(this Directions directions) => (directions & Directions.South) != Directions.None;

    /// <summary>
    /// Returns returns true if directions contain East.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool East(this Directions directions) => (directions & Directions.East) != Directions.None;

    /// <summary>
    /// Returns returns true if directions contain West.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool West(this Directions directions) => (directions & Directions.West) != Directions.None;

    /// <summary>
    /// Returns true if contains all of the dirs are present in original directions.
    /// </summary>
    /// <param name="directions"></param>
    /// <param name="dirs"></param>
    /// <returns></returns>
    public static bool Contains(this Directions directions, Directions dirs) => (directions & dirs) == dirs;

    /// <summary>
    /// Returns true if contains East or West, but does not contain North and East.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool Horizontal(this Directions directions) =>
        (directions.West() || directions.East())
        && !directions.North()
        && !directions.South();

    /// <summary>
    /// Returns true if contains North or South, but does not contain East adn West.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static bool Vertical(this Directions directions) =>
        (directions.North() || directions.South())
        && !directions.West()
        && !directions.East();

    /// <summary>
    /// If original directions contains horizontal directions, returns vertical directions and vice versa.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static Directions Perpendicular(this Directions directions)
    {
        var perpendicular = Directions.None;

        if (directions.North() || directions.South())
            perpendicular |= Directions.East | Directions.West;

        if (directions.East() || directions.West())
            perpendicular |= Directions.North | Directions.South;

        return perpendicular;
    }

    /// <summary>
    /// Returns original directions without given directions.
    /// </summary>
    /// <param name="directions"></param>
    /// <param name="without"></param>
    /// <returns></returns>
    public static Directions Without(this Directions directions, Directions without) => directions & ~without;

    /// <summary>
    /// Returns directions opposite to the original ones. N <-> S, E<->W
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static Directions Opposite(this Directions directions)
    {
        var opposite = Directions.None;
        if (directions.North()) opposite |= Directions.South;
        if (directions.South()) opposite |= Directions.North;
        if (directions.West()) opposite |= Directions.East;
        if (directions.East()) opposite |= Directions.West;

        return opposite;
    }

    /// <summary>
    /// Returns IEnumerable, each <see cref="Directions"/> containing only single direction.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static IEnumerable<Directions> Enumerate(this Directions directions)
    {
        if (directions.North()) yield return Directions.North;
        if (directions.West()) yield return Directions.West;
        if (directions.South()) yield return Directions.South;
        if (directions.East()) yield return Directions.East;
    }

    /// <summary>
    /// Returns direction in Unity world coordinates.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(this Directions directions)
    {
        Vector3 vector = new();
        if (directions.North()) vector += Vector3.forward;
        if (directions.South()) vector += Vector3.back;
        if (directions.West()) vector += Vector3.left;
        if (directions.East()) vector += Vector3.right;

        // This will be useful
        return vector;
    }

    /// <summary>
    /// Returns (x, y) direction.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static (int x, int y) ToCoords(this Directions directions)
    {
        int x = 0, y = 0;
        if (directions.North()) y += 1;
        if (directions.South()) y -= 1;
        if (directions.West()) x -= 1;
        if (directions.East()) x += 1;

        return (x, y);
    }

    /// <summary>
    /// Returns string of 'N' 'S' 'W' 'E' characters.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static string ToStr(this Directions directions)
    {
        StringBuilder sb = new();
        if (directions.North()) sb.Append('N');
        if (directions.South()) sb.Append('S');
        if (directions.East()) sb.Append('E');
        if (directions.West()) sb.Append('W');

        return sb.ToString();
    }

    /// <summary>
    /// Return directions randomly filled. Can contain all or none.
    /// </summary>
    /// <returns></returns>
    public static Directions GetRandom()
    {
        var dirs = Directions.None;
        if (URandom.value > 0.5f) dirs |= Directions.North;
        if (URandom.value > 0.5f) dirs |= Directions.South;
        if (URandom.value > 0.5f) dirs |= Directions.East;
        if (URandom.value > 0.5f) dirs |= Directions.West;

        if (dirs == Directions.None)
            return Directions.North;

        return dirs;
    }

    /// <summary>
    /// Returns a single direction from the original ones.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static Directions ChooseRandom(this Directions directions)
    {
        Directions[] dirs = directions.Enumerate().ToArray();
        int count = dirs.Length;
        if (count == 0) return Directions.None;

        return dirs[URandom.Range(0, count)];
    }

    /// <summary>
    /// Returns <see cref="Directions"/> containing every single direction.
    /// </summary>
    /// <returns></returns>
    public static Directions GetAll() =>
        Directions.South | Directions.North | Directions.East | Directions.West;
}
