using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DoorTile : EdgeTile
{
    public DoorLock Lock = null;
    public Directions DoorExits;

    public DoorTile(Directions edges, Directions freeExits, Directions doorExits, int thickness = 1) : base(edges, thickness, freeExits)
    {
        DoorExits = doorExits;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);
        List<(int, int, Directions dir)> doors = new();

        if (DoorExits.North()) doors.Add((x + HalfWidth, y, Directions.North));
        if (DoorExits.South()) doors.Add((x + HalfWidth, y + HEIGHT - 1, Directions.South));
        if (DoorExits.West()) doors.Add((x, y + HalfHeight, Directions.West));
        if (DoorExits.East()) doors.Add((x + WIDTH - 1, y + HalfHeight, Directions.East));

        foreach ((int dx, int dy, Directions dir)  in doors)
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            door.Orientation = dir;
            subTileGrid[dx, dy] = door;
        }
    }
}