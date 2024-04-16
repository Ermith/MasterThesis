using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Room : ASuperTile
{

    public Room(int width, int height, int floor, Directions exits = Directions.None) : base(width, height, floor, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        SuperTileDescription description = CreateDescription(x, y, tileGrid);
        BuildSubRoom(x, y, 0, 0, Width, Height, description, Exits, internalRoom: false);
        int midX = Width / 2;
        int midY = Height / 2;

        foreach ((Directions dir, (int ex, int ey)) in description.ExitsTiles)
        {
            var door = new DoorTile(
                EdgeDirectinons(ex, ey, Width, Height),
                Directions.None,
                dir);

            tileGrid[x + ex, y + ey] = door;
            door.Type = HasDefaultDoor.Contains(dir) ? DoorType.Door : DoorType.None;
        }


        List<(int, int)> patrol = new();

        foreach (Directions dir in Exits.Enumerate())
        {
            (int ex, int ey) = description.ExitsTiles[dir];
            foreach ((int px, int py) in GetShortPath(ex, ey, midX, midY))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                description.FreeTiles.Remove((px, py));
            }
        }

        description.PatrolPath = patrol;
        description.PatrolLooped = false;

        foreach (IKey k in Keys)
            k.Implement(description);

        foreach (ILock l in Locks)
            l.Implement(description);

        return description.Enemies;
    }
}

