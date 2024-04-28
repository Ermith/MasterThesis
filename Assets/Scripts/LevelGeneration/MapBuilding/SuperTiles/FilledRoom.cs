using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FilledRoom : ASuperTile
{
    bool _subRoom = false;
    int _floor = 0;
    bool _refuges = false;

    public FilledRoom(int width, int height, int floor, bool subRoom = false, Directions exits = Directions.None, bool refuges = false) : base(width, height, floor, exits)
    {
        _subRoom = subRoom;
        _refuges = refuges;
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        Description = CreateDescription(x, y, tileGrid);

        foreach ((Directions dir, (int ex, int ey)) in Description.ExitsTiles)
        {
            var door = new DoorTile(
                EdgeDirectinons(ex, ey, Width, Height),
                Directions.None,
                dir);

            door.RoomName = GetName();
            tileGrid[x + ex, y + ey] = door;
            door.Type = HasDefaultDoor.Contains(dir) ? DoorType.Door : DoorType.None;
        }


        foreach ((int ex, int ey) in EdgeLocations(Width, Height))
        {
            if (tileGrid[x + ex, y + ey] == null)
            {
                tileGrid[x + ex, y + ey] = new EdgeTile(EdgeDirectinons(ex, ey, Width, Height));
                Description.FreeTiles.Add((ex, ey));
            }
        }

        if (_subRoom)
            BuildSubRoom(
                x + 1, y + 1,
                1, 1,
                Width - 2, Height - 2,
                Description,
                DirectionsExtensions.GetAll(),
                internalRoom: true);
        else
            BuildWall(
                x + 1, y + 1,
                Width - 2, Height - 2,
                Description);

        if (_refuges)
            foreach ((int ex, int ey) in EdgeLocations(Width - 2, Height - 2))
            {
                if ((ex + ey) % 2 == 0)
                    continue;

                var dirs = EdgeDirectinons(ex, ey, Width - 2, Height - 2).Opposite();
                tileGrid[x + 1 + ex, y + 1 + ey] = new RefugeEdgeTile(dirs, dirs, thickness: ATile.WIDTH);
            }

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
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                Description.FreeTiles.Remove((px, py));
            }
        }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = true;

        foreach (IKey k in Keys)
            k?.Implement(Description);

        foreach (ILock l in Locks)
            l?.Implement(Description);

        return Description.Enemies;
    }
}