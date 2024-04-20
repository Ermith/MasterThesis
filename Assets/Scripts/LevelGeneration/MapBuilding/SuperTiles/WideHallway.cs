using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class WideHallway : ASuperTile
{
    public WideHallway(int width, int height, int floor, Directions exits)
        : base(width, height, floor, exits)
    {
        Exits = exits;
    }

    public override List<EnemyParams> BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        Description = CreateDescription(x, y, tileGrid);
        int midX = Width / 2;
        int midY = Height / 2;

        tileGrid[x + midX, y + midY] = new ColumnTile(DirectionsExtensions.GetAll());

        foreach ((int i, int j) in EdgeLocations(3, 3))
        {
            Directions edgeFlags = EdgeDirectinons(i, j, 3, 3);
            edgeFlags = edgeFlags.Without(Exits);

            tileGrid[x + midX + i - 1, y + midY + j - 1] = new EdgeTile(edgeFlags);
        }

        foreach ((Directions dir, (int ex, int ey)) in Description.ExitsTiles)
            foreach ((int px, int py) in GetShortPath(midX, midY, ex, ey))
            {
                // Neighbor 1 and Neighbor 2
                int nx1, ny1;
                int nx2, ny2;
                Directions edges1, edges2;

                if (dir.North() || dir.South())
                {
                    nx1 = px - 1; ny1 = py;
                    nx2 = px + 1; ny2 = py;

                    edges1 = EdgeDirectinons(0, ny1, 3, Height);
                    edges2 = EdgeDirectinons(2, ny2, 3, Height);

                } else
                {
                    nx1 = px; ny1 = py - 1;
                    nx2 = px; ny2 = py + 1;

                    edges1 = EdgeDirectinons(nx1, 0, Width, 3);
                    edges2 = EdgeDirectinons(nx2, 2, Width, 3);
                }

                tileGrid[x + nx1, y + ny1] ??= new EdgeTile(edges1);
                tileGrid[x + nx2, y + ny2] ??= new EdgeTile(edges2);
                //tileGrid[x + px, y + py] ??= new ColumnTile(dir.Opposite(), mid: false)
                tileGrid[x + px, y + py] ??= new EmptyTile();
            }


        List<(int, int)> patrol = new();
        List<(int, int)> keyPoints = new();

        keyPoints.Add((midX - 1, midY - 1));
        //*/
        if (Exits.North())
        {
            keyPoints.Add((midX - 1, 0));
            keyPoints.Add((midX + 1, 0));
        }
        //*/
        keyPoints.Add((midX + 1, midY - 1));
        //*/
        if (Exits.East())
        {
            keyPoints.Add((Width - 1, midY - 1));
            keyPoints.Add((Width - 1, midY + 1));
        }
        //*/
        keyPoints.Add((midX + 1, midY + 1));
        //*/
        if (Exits.South())
        {
            keyPoints.Add((midX + 1, Height - 1));
            keyPoints.Add((midX - 1, Height - 1));
        }
        //*/
        keyPoints.Add((midX - 1, midY + 1));
        //*/
        if (Exits.West())
        {
            keyPoints.Add((0, midY + 1));
            keyPoints.Add((0, midY - 1));
        }
        //*/
        

        for (int i = 0; i < keyPoints.Count; i++)
        {
            (int currentX, int currentY) = keyPoints[i];
            (int nextX, int nextY) = keyPoints[(i + 1) % keyPoints.Count];

            foreach ((int px, int py) in GetShortPath(currentX, currentY, nextX, nextY))
                patrol.Add(ATile.FromSuperMid(x + px, y + py));
        }

        Description.PatrolPath = patrol;
        Description.PatrolLooped = true;

        foreach (ILock l in Locks) l?.Implement(Description);
        foreach (IKey k in Keys) k?.Implement(Description);

        return Description.Enemies;
    }
}
