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
        Description = CreateDescription(x, y, tileGrid);
        BuildSubRoom(x, y, 0, 0, Width, Height, Description, Exits, internalRoom: false, refuges: true);
        int midX = Width / 2;
        int midY = Height / 2;

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


        List<(int, int)> patrol = new();

        foreach (Directions dir in Exits.Enumerate())
        {
            (int ex, int ey) = Description.ExitsTiles[dir];
            foreach ((int px, int py) in GetShortPath(ex, ey, midX, midY))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                Description.FreeTiles.Remove((px, py));
            }
        }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = false;

        foreach (IKey k in Keys)
            k.Implement(Description);

        foreach (ILock l in Locks)
            l.Implement(Description);

        return Description.Enemies;
    }
}

