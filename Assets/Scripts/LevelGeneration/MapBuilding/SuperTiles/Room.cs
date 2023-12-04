using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Room : ASuperTile
{

    public Room(int width, int height, Directions exits = Directions.None) : base(width, height, exits)
    {
    }

    public override EnemyParams BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        int midX = Width / 2;
        int midY = Height / 2;
        List<(int, int)> exits = new();
        if (Exits.North()) exits.Add((midX, 0));
        if (Exits.South()) exits.Add((midX, Height - 1));
        if (Exits.East()) exits.Add((Width - 1, midY));
        if (Exits.West()) exits.Add((0, midY));

        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
            {
                Directions edgeFlags = EdgeDirectinons(i, j, Width, Height);
                ATile tile;

                if (exits.Contains((i, j)))
                    tile = new DoorTile(edgeFlags);
                else if (!edgeFlags.None())
                    tile = new EdgeTile(edgeFlags);
                else
                    tile = new EmptyTile();

                tileGrid[x + i, y + j] = tile;
            }

        foreach (Key k in Keys)
            k.Implement(tileGrid, x, y, Width, Height);

        foreach (Lock l in Locks)
            l.Implement(tileGrid, x, y, Width, Height);

        List<(int, int)> patrol = new();

        if (Exits.West())
            foreach ((int px, int py) in GetShortPath(0, midY, midX, midY))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.North())
            foreach ((int px, int py) in GetShortPath(midX, 0, midX, midY))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.East())
            foreach ((int px, int py) in GetShortPath(Width - 1, midY, midX, midY))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.South())
            foreach ((int px, int py) in GetShortPath(midX, Height - 1, midX, midY))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        EnemyParams enemyParams = new();
        enemyParams.Patrol = patrol;
        enemyParams.Spawn = ((x + midX) * ATile.WIDTH + ATile.WIDTH / 2, (y + midY) * ATile.HEIGHT + ATile.HEIGHT / 2);
        return enemyParams;
    }
}

