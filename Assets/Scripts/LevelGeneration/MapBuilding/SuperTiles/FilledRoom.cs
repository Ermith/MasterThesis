using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FilledRoom : ASuperTile
{
    bool _subRoom = false;
    public FilledRoom(int width, int height, bool subRoom = false, Directions exits = Directions.None) : base(width, height, exits)
    {
        _subRoom = subRoom;
    }

    private void BuildSubRoom(int x, int y, int width, int height, ATile[,] tileGrid, Directions roomExits)
    {
        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
                tileGrid[i, j] = null;

        for (int i = 0; i < width; i++)
        {
            var edges = EdgeDirectinons(i, 0, width, height);
            var exits = (i == width / 2 && roomExits.North()) ? Directions.North : Directions.None;
            tileGrid[i + x, y] = new EdgeTile(edges, exits);

            edges = EdgeDirectinons(i, width - 1, width, height);
            exits = (i == width / 2 && roomExits.South()) ? Directions.South : Directions.None;
            tileGrid[i + x, y + height - 1] = new EdgeTile(edges, exits);
        }

        for (int i = 0; i < height; i++)
        {
            var edges = EdgeDirectinons(0, i, width, height);
            var exits = (i == height / 2 && roomExits.West()) ? Directions.West : Directions.None;
            var tile = tileGrid[x, i + y] as EdgeTile ?? new EdgeTile(edges);
            tile.Exits |= exits;
            tileGrid[x, i + y] = tile;


            edges = EdgeDirectinons(width - 1, i, width, height);
            exits = (i == height / 2 && roomExits.East()) ? Directions.East : Directions.None;

            tile = tileGrid[x + width - 1, i + y] as EdgeTile ?? new EdgeTile(edges);
            tile.Exits |= exits;
            tileGrid[x + width - 1, i + y] = tile;
        }

        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
                if (tileGrid[i, j] == null)
                    tileGrid[i, j] = new EmptyTile();
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

        Func<int, int, bool> isMid = (int x, int y) =>
            x > 0 && x < Width - 1
            && y > 0 && y < Height - 1;

        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
            {
                Directions edgeFlags = EdgeDirectinons(i, j, Width, Height);
                ATile tile;

                if (exits.Contains((i, j)))
                    tile = new DoorTile(edgeFlags);
                else if (!edgeFlags.None())
                    tile = new EdgeTile(edgeFlags);
                else if (isMid(i, j))
                    tile = new EdgeTile(DirectionsExtensions.GetAll());
                else
                    tile = new EmptyTile();

                tileGrid[x + i, y + j] = tile;
            }

        if (_subRoom)
            BuildSubRoom(x + 1, y + 1, Width - 2, Height - 2, tileGrid, Directions.East);

        foreach (Key k in Keys)
            k.Implement(tileGrid, x, y, Width, Height);

        foreach (Lock l in Locks)
            l.Implement(tileGrid, x, y, Width, Height);


        List<(int, int)> patrol = new();

        //if (Exits.West())
        //    foreach ((int px, int py) in GetShortPath(0, midY, midX, midY))
        //        patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));
        //
        //if (Exits.North())
        //    foreach ((int px, int py) in GetShortPath(midX, 0, midX, midY))
        //        patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));
        //
        //if (Exits.East())
        //    foreach ((int px, int py) in GetShortPath(Width - 1, midY, midX, midY))
        //        patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));
        //
        //if (Exits.South())
        //    foreach ((int px, int py) in GetShortPath(midX, Height - 1, midX, midY))
        //        patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        foreach ((int px, int py) in GetShortPath(0, midY, midX, 0, yFirst: true))
            patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        foreach ((int px, int py) in GetShortPath(midX, 0, Width - 1, midY, yFirst: false))
            patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        foreach ((int px, int py) in GetShortPath(Width - 1, midY, midX, Height - 1, yFirst: true))
            patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));

        foreach ((int px, int py) in GetShortPath(midX, Height - 1, 0, midY, yFirst: false))
            patrol.Add(((x + px) * ATile.WIDTH + ATile.WIDTH / 2, (y + py) * ATile.HEIGHT + ATile.HEIGHT / 2));


        EnemyParams enemyParams = new();
        enemyParams.Patrol = patrol;
        enemyParams.Spawn = patrol[0];
        return enemyParams;
    }
}