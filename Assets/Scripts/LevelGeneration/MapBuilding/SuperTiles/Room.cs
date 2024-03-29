﻿using System;
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

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);
        BuildSubRoom(x, y, 0, 0, Width, Height, description, Exits, internalRoom: false);
        int midX = Width / 2;
        int midY = Height / 2;

        List<(int, int)> patrol = new();

        foreach (Directions dir in Exits.Enumerate())
        {
            (int ex, int ey) = description.ExitsTiles[dir];
            foreach ((int px, int py) in GetShortPath(ex, ey, midX, midY))
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
        }

        description.PatrolPath = patrol;
        description.PatrolLooped = false;

        foreach (IKey k in Keys)
            k.Implement(description);

        foreach (ILock l in Locks)
            l.Implement(description);

        return description.Enemies;
    }
}

