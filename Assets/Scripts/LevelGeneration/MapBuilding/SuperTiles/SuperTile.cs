using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
} 

public abstract class ASuperTile
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Lock> Locks { get; set; }
    public List<Key> Keys { get; set;  }

    public Directions Exits { get; set; }

    public ASuperTile(int width, int height, Directions exits = Directions.None)
    {
        Width = width;
        Height = height;
        Exits = exits;
        Locks = new();
        Keys = new();
    }

    public abstract EnemyParams BuildTiles(int x, int y, ATile[,] tileGrid);

    internal IEnumerable<(int, int)> GetShortPath(int startX, int startY, int endX, int endY)
    {
        int step = MathF.Sign(endX - startX);
        for (int x = startX; x != endX; x += step)
            yield return (x, startY);

        step = MathF.Sign(endY - startY);
        for (int y = startY; y != endY; y += step)
            yield return (endX, y);

        yield return (endX, endY);
    }

}
