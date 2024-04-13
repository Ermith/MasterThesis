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
        SuperTileDescription description = CreateDescription(x, y, tileGrid);

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
            tileGrid[x + ex, y + ey] =
                new DoorTile(
                    EdgeDirectinons(ex, ey, Width, Height),
                    Directions.None,
                    dir);

        foreach ((int ex, int ey) in EdgeLocations(Width, Height))
        {
            tileGrid[x + ex, y + ey] ??= new EdgeTile(EdgeDirectinons(ex, ey, Width, Height));
        }

        BuildSubRoom(
                x + 1, y + 1,
                1, 1,
                Width - 2, Height - 2,
                description,
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
                description.FreeTiles.Remove((upX - x, i));
            }

            var up = tileGrid[upX, y + 1] as EdgeTile;
            tileGrid[upX, y + 1] = new DoorTile(up.Edges, Directions.None, upDir);
            tileGrid[upX, y + 2].Objects.Add(() => {
                var obj = GameObject.Instantiate(Blueprint);
                obj.transform.forward = Vector3.forward;
                return obj;
            });

            description.UpExit = (upX - x, 1);
            description.FreeTiles.Add((upX - x, Height - 2));
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
                description.FreeTiles.Remove((downX - x, i));
            }

            var down = tileGrid[downX, y + 3] as EdgeTile;
            tileGrid[downX, y + 3] = new DoorTile(down.Edges, Directions.None, downDir);
            (tileGrid[downX, y + 2] as EdgeTile).Floor = false;

            description.DownExit = (downX - x, 3);
        }


        foreach (ILock l in Locks) l.Implement(description);
        foreach (IKey k in Keys) k.Implement(description);

        return description.Enemies;
    }
}