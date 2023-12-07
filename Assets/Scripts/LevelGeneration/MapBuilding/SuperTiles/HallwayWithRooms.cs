using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HallwayWithRooms : Hallway
{
    public HallwayWithRooms(int width, int height, Directions exits = Directions.None) : base(width, height, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);

        int midX = Width / 2;
        int midY = Height / 2;
        List<(int, int)> patrol = new();

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
            foreach ((int px, int py) in GetShortPath(midX, midY, ex, ey))
            {
                if ((px, py) == (midX, midY)) continue;

                description.FreeTiles.Add((px, py));
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                tileGrid[x + px, y + py] = new EdgeTile(dir.Perpendicular()); ;
            }

        description.PatrolPath = patrol;

        Directions midWalls = ~Exits;
        tileGrid[x + midX, y + midY] = new EdgeTile(midWalls);
        description.FreeTiles.Add((midX, midY));

        Directions nwExits, neExits, swExits, seExits;
        nwExits = neExits = swExits = seExits = Directions.None;
        int roomWidth = (Width - 1) / 2;
        int roomHeight = (Height - 1) / 2;

        if (Exits.North())
        {
            nwExits |= Directions.East;
            neExits |= Directions.West;
            var tile = description.Get(midX, roomHeight / 2) as EdgeTile;
            tile.Exits |= nwExits | neExits;
        }

        if (Exits.South())
        {
            swExits |= Directions.East;
            seExits |= Directions.West;
            var tile = description.Get(midX, midY + 1 + roomHeight / 2) as EdgeTile;
            tile.Exits |= swExits | seExits;
        }

        if (Exits.West())
        {
            nwExits |= Directions.South;
            swExits |= Directions.North;
            var tile = description.Get(roomWidth / 2, midY) as EdgeTile;
            tile.Exits |= nwExits | swExits;
        }

        if (Exits.East())
        {
            neExits |= Directions.South;
            seExits |= Directions.North;
            var tile = description.Get(midX + 1 + roomWidth / 2, midY) as EdgeTile;
            tile.Exits |= neExits | seExits;
        }

        if (!nwExits.None())
            BuildSubRoom(
                x, y,
                roomWidth, roomHeight,
                description, nwExits);

        if (!neExits.None()) 
            BuildSubRoom(
                x + midX + 1, y,
                roomWidth, roomHeight,
                description, neExits);

        if (!swExits.None())
            BuildSubRoom(
                x, y + midY + 1,
                roomWidth, roomHeight,
                description, swExits);

        if (!seExits.None())
            BuildSubRoom(
                x + midX + 1, y + midY + 1,
                roomWidth, roomHeight,
                description, seExits);

        foreach (Lock l in Locks) l.Implement(description);
        foreach (Key k in Keys) k.Implement(description);

        return description.Enemies;
    }
}