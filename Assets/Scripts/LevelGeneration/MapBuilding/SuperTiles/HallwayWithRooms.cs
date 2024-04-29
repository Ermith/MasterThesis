using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Hallway layout with liitle rooms at the corners.
/// </summary>
public class HallwayWithRooms : Hallway
{
    public HallwayWithRooms(int width, int height, int floor, in Directions exits = Directions.None) : base(width, height, floor, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        Description = CreateDescription(x, y, tileGrid);

        int midX = Width / 2;
        int midY = Height / 2;
        List<(int, int)> patrol = new();

        // Spawn the hallway itself
        foreach ((Directions dir, (int ex, int ey)) in Description.ExitsTiles)
            foreach ((int px, int py) in GetShortPath(midX, midY, ex, ey))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));

                if ((px, py) == (midX, midY)) continue;

                tileGrid[x + px, y + py] = new EdgeTile(dir.Perpendicular());
            }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = false;

        Directions midWalls = ~Exits;
        tileGrid[x + midX, y + midY] = new EdgeTile(midWalls);

        // Determine exits of each room
        Directions nwExits, neExits, swExits, seExits;
        nwExits = neExits = swExits = seExits = Directions.None;
        int roomWidth = (Width - 1) / 2;
        int roomHeight = (Height - 1) / 2;

        if (Exits.North())
        {
            nwExits |= Directions.East;
            neExits |= Directions.West;
            var tile = Description.Get(midX, midY + 1 + roomHeight / 2) as EdgeTile;
            tile.Exits |= nwExits | neExits;
        }

        if (Exits.South())
        {
            swExits |= Directions.East;
            seExits |= Directions.West;
            var tile = Description.Get(midX, roomHeight / 2) as EdgeTile;
            tile.Exits |= swExits | seExits;
        }

        if (Exits.West())
        {
            nwExits |= Directions.South;
            swExits |= Directions.North;
            var tile = Description.Get(roomWidth / 2, midY) as EdgeTile;
            tile.Exits |= nwExits | swExits;
        }

        if (Exits.East())
        {
            neExits |= Directions.South;
            seExits |= Directions.North;
            var tile = Description.Get(midX + 1 + roomWidth / 2, midY) as EdgeTile;
            tile.Exits |= neExits | seExits;
        }

        // Spawn the subrooms

        if (!swExits.None())
            BuildSubRoom(
                x, y,
                0, 0,
                roomWidth, roomHeight,
                Description, swExits);

        if (!seExits.None()) 
            BuildSubRoom(
                x + midX + 1, y,
                midX + 1, 0,
                roomWidth, roomHeight,
                Description, seExits);

        if (!nwExits.None())
            BuildSubRoom(
                x, y + midY + 1,
                0, midY + 1,
                roomWidth, roomHeight,
                Description, nwExits);

        if (!neExits.None())
            BuildSubRoom(
                x + midX + 1, y + midY + 1,
                midX + 1, midY + 1,
                roomWidth, roomHeight,
                Description, neExits);

        foreach (ILock l in Locks) l?.Implement(Description);
        foreach (IKey k in Keys) k?.Implement(Description);

        return Description.Enemies;
    }
}