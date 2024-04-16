using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using URandom = UnityEngine.Random;

public class Hallway : ASuperTile
{
    public Hallway(int width, int height, int floor, Directions exits = Directions.None) : base(width, height, floor, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        Description = CreateDescription(x, y, tileGrid);

        int midX = Width / 2;
        int midY = Height / 2;
        List<(int, int)> patrol = new();

        foreach ((Directions dir, (int ex, int ey)) in Description.ExitsTiles)
            foreach ((int px, int py) in GetShortPath(midX, midY, ex, ey))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                if ((px, py) == (midX, midY)) continue;

                //description.FreeTiles.Add((px, py));

                if ((px + py) % 2 == 0)
                    tileGrid[x + px, y + py] = new EdgeTile(dir.Perpendicular(), thickness: 2);
                else
                    tileGrid[x + px, y + py] = new RefugeEdgeTile(dir.Perpendicular(), dir.Perpendicular(), thickness: 2);
            }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = false;
        

        Directions midWalls = ~Exits;
        tileGrid[x + midX, y + midY] = new RefugeEdgeTile(midWalls, midWalls, thickness: 2);
        Description.FreeTiles.Add((midX, midY));

        foreach (ILock l in Locks) l.Implement(Description);
        foreach (IKey k in Keys) k.Implement(Description);

        return Description.Enemies;
    }
}