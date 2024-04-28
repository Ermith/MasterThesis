using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class StairwayRoom : ASuperTile
{
    public static GameObject Blueprint;
    private bool _reversed;
    private bool _upExit;
    private bool _downExit;

    public StairwayRoom(int width, int height, int floor, Directions exits = Directions.None, bool reveresed = false, bool up = false, bool down = false) : base(width, height, floor, exits)
    {
        _reversed = reveresed;
        _upExit = up;
        _downExit = down;
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
                Description.PatrolPath.Add((ex, ey));
            }
        }

        BuildSubRoom(
                x + 1, y + 1,
                1, 1,
                Width - 2, Height - 2,
                Description,
                Directions.North | Directions.South,
                internalRoom: true);

        if (_upExit)
        {
            Directions upDir = _reversed ? Directions.West : Directions.East;
            int upX = _reversed
            ? x + Width - 2
            : x + 1;

            for (int i = 1; i <= Height - 2; i++)
            {
                (tileGrid[upX, y + i] as EdgeTile).Edges |= upDir;
                Description.FreeTiles.Remove((upX - x, i));
            }

            var up = tileGrid[upX, y + 1] as EdgeTile;
            var door = new DoorTile(up.Edges, Directions.None, upDir);
            door.Up = true;
            door.RoomName = GetName();
            tileGrid[upX, y + 1] = door;
            tileGrid[upX, y + 2].Objects.Add(() => {
                var obj = GameObject.Instantiate(Blueprint);
                obj.transform.forward = Vector3.forward;
                return obj;
            });

            Description.UpExit = (upX - x, 1);
        }

        if (_downExit)
        {
            Directions downDir = _reversed ? Directions.East : Directions.West;
            int downX = _reversed
            ? x + 1
            : x + Width - 2;

            for (int i = 1; i <= Height - 2; i++)
            {
                (tileGrid[downX, y + i] as EdgeTile).Edges |= downDir;
                Description.FreeTiles.Remove((downX - x, i));
            }

            var down = tileGrid[downX, y + 3] as EdgeTile;
            var door = new DoorTile(down.Edges, Directions.None, downDir);
            door.Down = true;
            door.RoomName = GetName();
            tileGrid[downX, y + 3] = door;
            (tileGrid[downX, y + 2] as EdgeTile).Floor = false;

            Description.DownExit = (downX - x, 3);
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


        foreach (ILock l in Locks) l?.Implement(Description);
        foreach (IKey k in Keys) k?.Implement(Description);

        return Description.Enemies;
    }
}