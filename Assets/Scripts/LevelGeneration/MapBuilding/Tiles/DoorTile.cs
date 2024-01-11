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

    public DoorTile(Directions edges, Directions freeExits, Directions doorExits) : base(edges, freeExits)
    {
        DoorExits = doorExits;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);
        List<(int, int, Directions dir)> doors = new();
        if (DoorExits.North()) doors.Add((x + 1, y, Directions.North));
        if (DoorExits.South()) doors.Add((x + 1, y + 2, Directions.South));
        if (DoorExits.West()) doors.Add((x, y + 1, Directions.West));
        if (DoorExits.East()) doors.Add((x + 2, y + 1, Directions.East));

        foreach ((int dx, int dy, Directions dir)  in doors)
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            door.Orientation = dir;
            subTileGrid[dx, dy] = door;
        }
    }
}