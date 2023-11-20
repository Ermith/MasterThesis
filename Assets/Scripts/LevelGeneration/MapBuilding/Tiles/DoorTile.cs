using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DoorTile : ATile
{
    Directions DoorExit;
    public DoorLock Lock = null;

    public DoorTile(Directions doorExit)
    {
        DoorExit = doorExit;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        if (DoorExit.West())
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            subTileGrid[x, y + 0] = new WallSubTile();
            subTileGrid[x, y + 1] = door;
            subTileGrid[x, y + 2] = new WallSubTile();
        }

        if (DoorExit.East())
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            subTileGrid[x + 2, y + 0] = new WallSubTile();
            subTileGrid[x + 2, y + 1] = door;
            subTileGrid[x + 2, y + 2] = new WallSubTile();
        }

        if (DoorExit.North())
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            subTileGrid[x + 0, y] = new WallSubTile();
            subTileGrid[x + 1, y] = door;
            subTileGrid[x + 2, y] = new WallSubTile();
        }

        if (DoorExit.South())
        {
            DoorSubTile door = new();
            door.DoorLock = Lock;
            subTileGrid[x + 0, y + 2] = new WallSubTile();
            subTileGrid[x + 1, y + 2] = door;
            subTileGrid[x + 2, y + 2] = new WallSubTile();
        }

        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                if (subTileGrid[x + i, y + j] == null)
                    subTileGrid[x + i, y + j] = new FloorSubTile();
            }

        subTileGrid[x + 1, y + 1].Objects = Objects;
    }
}