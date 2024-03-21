using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum DoorType
{
    Door,
    WallOfLight,
}

public class DoorTile : EdgeTile
{
    public ILock Lock = null;
    public Directions DoorExits;
    public DoorType Type;

    public DoorTile(Directions edges, Directions freeExits, Directions doorExits, int thickness = 1, DoorType type = DoorType.Door) : base(edges, thickness, freeExits)
    {
        DoorExits = doorExits;
        Type = type;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);
        List<(int, int, Directions dir)> doors = new();

        if (DoorExits.South()) doors.Add((x + HalfWidth, y, Directions.North));
        subTileGrid[x + HalfWidth - 1, y] = new FloorSubTile();

        if (DoorExits.North()) doors.Add((x + HalfWidth, y + HEIGHT - 1, Directions.North));
        subTileGrid[x + HalfWidth - 1, y + HEIGHT - 1] = new FloorSubTile();

        if (DoorExits.West()) doors.Add((x, y + HalfHeight, Directions.East));
        subTileGrid[x, y + HalfHeight - 1] = new FloorSubTile();

        if (DoorExits.East()) doors.Add((x + WIDTH - 1, y + HalfHeight, Directions.East));
        subTileGrid[x + WIDTH - 1, y + HalfHeight - 1] = new FloorSubTile();

        if (Type == DoorType.Door)
            foreach ((int dx, int dy, Directions dir) in doors)
            {
                DoorSubTile door = new();
                door.DoorLock = Lock as DoorLock;
                door.Orientation = dir;
                subTileGrid[dx, dy] = door;
            }

        if (Type == DoorType.WallOfLight)
            foreach ((int dx, int dy, Directions dir) in doors)
            {
                WallOfLightSubTile door = new();
                door.Lock = Lock as WallOfLightLock;
                door.Orientation = dir;
                subTileGrid[dx, dy] = door;
            }
    }
}