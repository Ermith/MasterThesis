using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WideHallway : ASuperTile
{
    public WideHallway(int width, int height, Directions exits)
        : base(width, height, exits)
    {
        Exits = exits;
    }

    public override EnemyParams BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        int xmid = x + Width / 2;
        int ymid = y + Height / 2;

        if (Exits.North())
            for (int i = y; i < ymid; i++)
            {
                ATile midTile = i % 2 == 1
                    ? new ColumnTile(Directions.None)
                    : new EmptyTile();

                EdgeTile westTile = new(Directions.West);
                EdgeTile eastTile = new(Directions.East);

                if (i == y)
                {
                    westTile.Edges |= Directions.North;
                    eastTile.Edges |= Directions.North;
                }

                tileGrid[xmid, i] = midTile;
                tileGrid[xmid - 1, i] = westTile;
                tileGrid[xmid + 1, i] = eastTile;
            }

        if (Exits.South())
            for (int i = ymid + 1; i < y + Height; i++)
            {
                ATile midTile = i % 2 == 0
                    ? new ColumnTile(Directions.None)
                    : new EmptyTile();

                tileGrid[xmid, i] = midTile;
                EdgeTile t1 = new(Directions.West);
                EdgeTile t2 = new(Directions.East);

                if (i == y + Height - 1)
                {
                    t1.Edges |= Directions.South;
                    t2.Edges |= Directions.South;
                }

                tileGrid[xmid - 1, i] = t1;
                tileGrid[xmid + 1, i] = t2;
            }

        if (Exits.West())
            for (int i = x; i < xmid; i++)
            {
                ATile midTile = i % 2 == 0
                    ? new ColumnTile(Directions.None)
                    : new EmptyTile();

                EdgeTile northTile = new(Directions.North);
                EdgeTile southTile = new(Directions.South);

                if (i == x)
                {
                    northTile.Edges |= Directions.West;
                    southTile.Edges |= Directions.West;
                }

                tileGrid[i, ymid] = midTile;
                tileGrid[i, ymid - 1] = northTile;
                tileGrid[i, ymid + 1] = southTile;
            }

        if (Exits.East())
            for (int i = xmid + 1; i < x + Width; i++)
            {
                ATile midTile = i % 2 == 0
                    ? new ColumnTile(Directions.None)
                    : new EmptyTile();

                EdgeTile northTile = new(Directions.North);
                EdgeTile southTile = new(Directions.South);

                if (i == x + Width - 1)
                {
                    northTile.Edges |= Directions.East;
                    southTile.Edges |= Directions.East;
                }

                tileGrid[i, ymid] = midTile;
                tileGrid[i, ymid - 1] = northTile;
                tileGrid[i, ymid + 1] = southTile;
            }

        if (tileGrid[xmid - 1, ymid] == null)
            tileGrid[xmid - 1, ymid] = new EdgeTile(Directions.West);

        if (tileGrid[xmid + 1, ymid] == null)
            tileGrid[xmid + 1, ymid] = new EdgeTile(Directions.East);

        if (tileGrid[xmid, ymid - 1] == null)
            tileGrid[xmid, ymid - 1] = new EdgeTile(Directions.North);

        if (tileGrid[xmid, ymid + 1] == null)
            tileGrid[xmid, ymid + 1] = new EdgeTile(Directions.South);


        if (tileGrid[xmid - 1, ymid - 1] == null)
            tileGrid[xmid - 1, ymid - 1] = new EdgeTile(Directions.West | Directions.North);

        if (tileGrid[xmid + 1, ymid - 1] == null)
            tileGrid[xmid + 1, ymid - 1] = new EdgeTile(Directions.East | Directions.North);

        if (tileGrid[xmid - 1, ymid + 1] == null)
            tileGrid[xmid - 1, ymid + 1] = new EdgeTile(Directions.West | Directions.South);

        if (tileGrid[xmid + 1, ymid + 1] == null)
            tileGrid[xmid + 1, ymid + 1] = new EdgeTile(Directions.East | Directions.South);

        tileGrid[xmid, ymid] = new ColumnTile(
            Directions.North
            | Directions.South
            | Directions.West
            | Directions.East
            );



        List<(int, int)> patrol = new();
        xmid = Width / 2;
        ymid = Height / 2;

        if (Exits.West())
            foreach ((int px, int py) in GetShortPath(0, ymid, xmid, ymid))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py + 1) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.North())
            foreach ((int px, int py) in GetShortPath(xmid, 0, xmid, ymid))
                patrol.Add(((x + px + 1) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.East())
            foreach ((int px, int py) in GetShortPath(Width - 1, ymid, xmid, ymid))
                patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py + 1) * ATile.HEIGHT + ATile.HEIGHT / 2));

        if (Exits.South())
            foreach ((int px, int py) in GetShortPath(xmid, Height - 1, xmid, ymid))
                patrol.Add(((x + px + 1) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        EnemyParams enemyParams = new();
        enemyParams.Patrol = patrol;
        enemyParams.Spawn = patrol[0];
        return enemyParams;
    }
}
