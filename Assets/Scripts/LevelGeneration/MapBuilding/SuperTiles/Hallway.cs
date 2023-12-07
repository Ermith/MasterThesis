﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Hallway : ASuperTile
{
    public Hallway(int width, int height, Directions exits = Directions.None) : base(width, height, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);

        int midX = Width / 2;
        int midY = Height / 2;
        List<(int, int)> patrol = new();

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
            foreach ((int px, int py) in GetShortPath(midX, midY, ex, ey))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                if ((px, py) == (midX, midY)) continue;

                description.FreeTiles.Add((px, py));
                tileGrid[x + px, y + py] = new EdgeTile(dir.Perpendicular());
            }

        description.PatrolPath = patrol;
        

        Directions midWalls = ~Exits;
        tileGrid[x + midX, y + midY] = new EdgeTile(midWalls);
        description.FreeTiles.Add((midX, midY));

        foreach (Lock l in Locks) l.Implement(description);
        foreach (Key k in Keys) k.Implement(description);

        return description.Enemies;
    }
}