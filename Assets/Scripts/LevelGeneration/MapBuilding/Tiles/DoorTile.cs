﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum DoorType
{
    None,
    Door,
    WallOfLight,
    HiddenDoor
}

/// <summary>
/// Tile containing exits. Can be either empty space or a certain type of a door. Door usually spans two subtiles.
/// </summary>
public class DoorTile : EdgeTile
{
    public ILock Lock = null;
    public Directions DoorExits;
    public DoorType Type;
    public string RoomName;
    public bool Up;
    public bool Down;

    public DoorTile(Directions edges, Directions freeExits, Directions doorExits, int thickness = 1, DoorType type = DoorType.None) : base(edges, thickness, freeExits)
    {
        DoorExits = doorExits;
        Type = type;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);
        List<(int, int, Directions dir)> doors = new();

        // Exits take 2 subtiles

        if (DoorExits.South()) doors.Add((x + HalfWidth, y, Directions.South));
        if (DoorExits.South()) subTileGrid[x + HalfWidth - 1, y] = new FloorSubTile();

        if (DoorExits.North()) doors.Add((x + HalfWidth - 1, y + HEIGHT - 1, Directions.North));
        if (DoorExits.North()) subTileGrid[x + HalfWidth, y + HEIGHT - 1] = new FloorSubTile();

        if (DoorExits.West()) doors.Add((x, y + HalfHeight -1, Directions.West));
        if (DoorExits.West()) subTileGrid[x, y + HalfHeight] = new FloorSubTile();

        if (DoorExits.East()) doors.Add((x + WIDTH - 1, y + HalfHeight, Directions.East));
        if (DoorExits.East()) subTileGrid[x + WIDTH - 1, y + HalfHeight - 1] = new FloorSubTile();

        // Just empty space to pass through
        if (Type == DoorType.None)
            foreach((int dx, int dy, Directions _) in doors)
            {
                subTileGrid[dx, dy] = new FloorSubTile();
            }

        if (Type == DoorType.Door)
            foreach ((int dx, int dy, Directions dir) in doors)
            {
                DoorSubTile door = new();
                door.DoorLock = Lock as DoorLock;
                door.Orientation = dir;
                door.Up = Up;
                door.Down = Down;
                door.SetName(RoomName);
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

        if (Type == DoorType.HiddenDoor)
            foreach ((int dx, int dy, Directions dir) in doors)
            {
                HiddenDoorSubTile door = new();
                door.Orientation = dir;
                subTileGrid[dx, dy] = door;
            }
    }
}