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

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
            tileGrid[x + ex, y + ey] =
                new DoorTile(
                    EdgeDirectinons(ex, ey, Width, Height),
                    Directions.None,
                    dir);

        foreach ((int ex, int ey) in EdgeLocations(Width, Height))
            tileGrid[x + ex, y + ey] ??= new EdgeTile(EdgeDirectinons(ex, ey, Width, Height));

        if (_subRoom)
            BuildSubRoom(
                x + 1, y + 1,
                Width - 2, Height - 2,
                description,
                DirectionsExtensions.GetRandom(),
                internalRoom: true);
        else
            BuildWall(
                x + 1, y + 1,
                Width - 2, Height - 2,
                description);

        var corners = new (int, int)[] {
            (0, 0),
            (0, Height - 1),
            (Width - 1, Height - 1),
            (Width - 1, 0)
        };

        List<(int, int)> patrol = new();
        for (int i = 0; i < 4; i++)
        {
            (int cx, int cy) = corners[i];
            (int nextCx, int nextCy) = corners[(i + 1) % 4];
            foreach ((int px, int py) in GetShortPath(cx, cy, nextCx, nextCy))
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
        }

        description.PatrolPath = patrol;
        description.PatrolLooped = true;

        foreach (Key k in Keys)
            k.Implement(description);

        foreach (Lock l in Locks)
            l.Implement(description);

        return description.Enemies;
    }
}