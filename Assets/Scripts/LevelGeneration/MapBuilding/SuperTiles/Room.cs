﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Room : ASuperTile
{
    private Directions EdgeDirectinons(int x, int y)
    {
        Directions directions = Directions.None;

        if (x == 0) directions |= Directions.West;
        if (y == 0) directions |= Directions.North;
        if (x == Width - 1) directions |= Directions.East;
        if (y == Height - 1) directions |= Directions.South;

        return directions;
    }

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

                Directions edgeFlags = EdgeDirectinons(i, j);

                tileGrid[x + i, y + j] =
                    exits.Contains((i, j)) || edgeFlags.None()
                    ? new EmptyTile()
                    : new EdgeTile(edgeFlags);
            }

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
