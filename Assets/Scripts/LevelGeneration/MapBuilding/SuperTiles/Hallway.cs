using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Hallway : ASuperTile
{
    public Hallway(int width, int height, Directions exits = Directions.None) : base(width, height, exits)
    {
    }

    public override EnemyParams BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        int midX = Width / 2;
        int midY = Height / 2;

        for (int i = 0; i < Width; i++)
        {
            if (i < midX && Exits.West()) tileGrid[x + i, y + midY] = new EdgeTile(Directions.North | Directions.South);
            if (i > midX && Exits.East()) tileGrid[x + i, y + midY] = new EdgeTile(Directions.North | Directions.South);
        }

        for (int j = 0; j < Width; j++)
        {
            if (j < midY && Exits.North()) tileGrid[x + midX, y + j] = new EdgeTile(Directions.West | Directions.East);
            if (j > midY && Exits.South()) tileGrid[x + midX, y + j] = new EdgeTile(Directions.West | Directions.East);
        }

        Directions midWalls = Directions.None;
        if (!Exits.North()) midWalls |= Directions.North;
        if (!Exits.South()) midWalls |= Directions.South;
        if (!Exits.East()) midWalls |= Directions.East;
        if (!Exits.West()) midWalls |= Directions.West;
        
        tileGrid[x + midX, y + midY] = new EdgeTile(midWalls);

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