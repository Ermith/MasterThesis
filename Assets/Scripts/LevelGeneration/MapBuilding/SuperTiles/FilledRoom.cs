using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FilledRoom : ASuperTile
{
    bool _subRoom = false;
    int _floor = 0;

    public FilledRoom(int width, int height, int floor, bool subRoom = false, Directions exits = Directions.None) : base(width, height, floor, exits)
    {
        _subRoom = subRoom;
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
        {
            var door = new DoorTile(
                EdgeDirectinons(ex, ey, Width, Height),
                Directions.None,
                dir);

            tileGrid[x + ex, y + ey] = door;
            door.Type = HasDefaultDoor.Contains(dir) ? DoorType.Door : DoorType.None;
        }


        foreach ((int ex, int ey) in EdgeLocations(Width, Height))
        {
            tileGrid[x + ex, y + ey] ??= new EdgeTile(EdgeDirectinons(ex, ey, Width, Height));
            description.FreeTiles.Add((ex, ey));
        }

        if (_subRoom)
            BuildSubRoom(
                x + 1, y + 1,
                1, 1,
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

        foreach (IKey k in Keys)
            k.Implement(description);

        foreach (ILock l in Locks)
            l.Implement(description);

        return description.Enemies;
    }
}