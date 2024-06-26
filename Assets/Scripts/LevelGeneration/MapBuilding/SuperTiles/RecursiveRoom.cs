﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Room inside of another room.
/// </summary>
public class RecursiveRoom : ASuperTile
{
    public RecursiveRoom(int width, int height, int floor, Directions exits = Directions.None) : base(width, height, floor, exits)
    {
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        Description = CreateDescription(x, y, tileGrid);

        // Spawn doors tiles.
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

        // Spawns edges on the outside.
        foreach ((int ex, int ey) in EdgeLocations(Width, Height))
        {
            if (tileGrid[x + ex, y + ey] == null)
            {
                tileGrid[x + ex, y + ey] = new EdgeTile(EdgeDirectinons(ex, ey, Width, Height));
                Description.FreeTiles.Add((ex, ey));
            }
        }

        // Spawn the inner room.
        BuildSubRoom(
            x + 1, y + 1,
            1, 1,
            Width - 2, Height - 2,
            Description,
            DirectionsExtensions.GetAll(),
            internalRoom: true);

        // Determine Patrol Path
        var corners = new (int, int)[] {
            (0, 0),
            (0, Height - 1),
            (Width - 1, Height - 1),
            (Width - 1, 0)
        };

        List<(int, int)> patrol = new();
        for (int i = 0; i < 4; i++)
        {
            (int cx, int cy) = corners[i];
            (int nextCx, int nextCy) = corners[(i + 1) % 4];
            foreach ((int px, int py) in GetShortPath(cx, cy, nextCx, nextCy))
            {
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
                Description.FreeTiles.Remove((px, py));
            }
        }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = true;

        foreach (IKey k in Keys)
            k?.Implement(Description);

        foreach (ILock l in Locks)
            l?.Implement(Description);

        return Description.Enemies;
    }
}