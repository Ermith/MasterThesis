using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using URandom = UnityEngine.Random;

[Flags]
public enum Directions
{
    None = 0b0,
    North = 0b1,
    South = 0b10,
    East = 0b100,
    West = 0b1000,
}

public static class DirectionsExtensions
{
    public static bool None(this Directions directions) => directions == Directions.None;
    public static bool North(this Directions directions) => (directions & Directions.North) != Directions.None;
    public static bool South(this Directions directions) => (directions & Directions.South) != Directions.None;
    public static bool East(this Directions directions) => (directions & Directions.East) != Directions.None;
    public static bool West(this Directions directions) => (directions & Directions.West) != Directions.None;
    public static bool Horizontal(this Directions directions) =>
        (directions.West() || directions.East())
        && !directions.North()
        && !directions.South();

    public static bool Vertical(this Directions directions) =>
        (directions.North() || directions.South())
        && !directions.West()
        && !directions.East();

    public static Directions Perpendicular(this Directions directions)
    {
        var perpendicular = Directions.None;

        if (directions.North() || directions.South())
            perpendicular |= Directions.East | Directions.West;

        if (directions.East() || directions.West())
            perpendicular |= Directions.North | Directions.South;

        return perpendicular;
    }

    public static Directions Without(this Directions directions, Directions without) => directions & ~without;

    public static Directions Opposite(this Directions directions)
    {
        var opposite = Directions.None;
        if (directions.North()) opposite |= Directions.South;
        if (directions.South()) opposite |= Directions.North;
        if (directions.West()) opposite |= Directions.East;
        if (directions.East()) opposite |= Directions.West;

        return opposite;
    }

    public static IEnumerable<Directions> Enumerate(this Directions directions)
    {
        if (directions.North()) yield return Directions.North;
        if (directions.West()) yield return Directions.West;
        if (directions.South()) yield return Directions.South;
        if (directions.East()) yield return Directions.East;
    }

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

    public static Directions GetAll() =>
        Directions.South | Directions.North | Directions.East | Directions.West;
}
