using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using URandom = UnityEngine.Random;

public class SuperTileDescription
{
    public HashSet<(int, int)> FreeTiles = new();
    public Dictionary<Directions, (int, int)> ExitsTiles = new();
    public HashSet<(int, int)> InternalExits = new();
    public List<(int, int)> PatrolPath = new();
    public List<EnemyParams> Enemies = new();
    public int X, Y;
    public int Width, Height;
    public ATile[,] TileGrid;
    public Directions Exits;
    public bool PatrolLooped;
    
    public ATile Get(int x, int y) => TileGrid[x + X, y + Y];
}


public abstract class ASuperTile
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Lock> Locks { get; set; }
    public List<Key> Keys { get; set;  }

    public Directions Exits { get; set; }
    public Dictionary<Directions, (int, int)> ExitTiles;
    public List<(int, int)> InternalExitTiles;

    public ASuperTile(int width, int height, Directions exits = Directions.None)
    {
        Width = width;
        Height = height;
        Exits = exits;
        Locks = new();
        Keys = new();
        ExitTiles = new();
        InternalExitTiles = new();
    }

    internal Directions EdgeDirectinons(int x, int y, int width, int height)
    {
        Directions directions = Directions.None;

        if (x == 0) directions |= Directions.West;
        if (y == 0) directions |= Directions.North;
        if (x == width - 1) directions |= Directions.East;
        if (y == height - 1) directions |= Directions.South;

        return directions;
    }

    public abstract List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid);

    internal IEnumerable<(int, int)> GetShortPath(int startX, int startY, int endX, int endY, bool yFirst = false)
    {

        if (yFirst)
        {
            int step = MathF.Sign(endY - startY);
            for (int y = startY; y != endY; y += step)
                yield return (startX, y);

            step = MathF.Sign(endX - startX);
            for (int x = startX; x != endX; x += step)
                yield return (x, endY);

        } else
        {
            int step = MathF.Sign(endX - startX);
            for (int x = startX; x != endX; x += step)
                yield return (x, startY);

            step = MathF.Sign(endY - startY);
            for (int y = startY; y != endY; y += step)
                yield return (endX, y);
        }


        yield return (endX, endY);
    }

    internal SuperTileDescription CreateDescription(int x, int y, ATile[,] tileGrid)
    {
        int midX = Width / 2;
        int midY = Height / 2;
        Dictionary<Directions, (int, int)> exits = new();
        if (Exits.North()) exits[Directions.North] = (midX, 0);
        if (Exits.South()) exits[Directions.South] = (midX, Height - 1);
        if (Exits.East()) exits[Directions.East] = (Width - 1, midY);
        if (Exits.West()) exits[Directions.West] = (0, midY);

        return new SuperTileDescription()
        {
            TileGrid = tileGrid,
            X = x,
            Y = y,
            Width = Width,
            Height = Height,
            Exits = Exits,
            ExitsTiles = exits
        };
    }

    internal IEnumerable<(int, int)> EdgeLocations(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            yield return (x, 0);
            yield return (x, height - 1);
        }

        for (int y = 1; y < height - 1;  y++)
        {
            yield return (0, y);
            yield return (width - 1, y);
        }
    }

    internal void BuildSubRoom(
        int x, int y,
        int width, int height,
        SuperTileDescription description,
        Directions roomExits, bool internalRoom = true)
    {
        ATile[,] tileGrid = description.TileGrid;

        int midX = width / 2;
        int midY = height / 2;

        Dictionary<Directions, (int, int)> exits = new();
        if (roomExits.North()) exits[Directions.North] = (midX, 0);
        if (roomExits.South()) exits[Directions.South] = (midX, height - 1);
        if (roomExits.East()) exits[Directions.East] = (width - 1, midY);
        if (roomExits.West()) exits[Directions.West] = (0, midY);


        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                Directions edgeFlags = EdgeDirectinons(i, j, width, height);
                ATile tile = tileGrid[x + i, y + j];
                
                if (!edgeFlags.None())
                {
                    tile = new EdgeTile(edgeFlags);
                } else
                {
                    tile = new EmptyTile();
                    description.FreeTiles.Add((i, j));
                }

                tileGrid[x + i, y + j] = tile;
            }

        foreach ((Directions dir, (int ex, int ey)) in exits)
        {
            Directions edgeFlags = EdgeDirectinons(ex, ey, width, height);
            DoorTile door = new(edgeFlags, Directions.None, dir);
            if (internalRoom) description.InternalExits.Add((ex, ey));
            tileGrid[x + ex, y + ey] = door;
        }
    }

    internal void BuildWall(int x, int y, int width, int height, SuperTileDescription description)
    {
        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
                description.TileGrid[i, j] = new WallTile();
    }
}
