using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EdgeTile : ATile
{
    public Directions Edges { get; set; }
    public Directions Exits { get; set; }
    public int Thickness { get; private set; }

    public EdgeTile(Directions edges, int thickness = 1, Directions exits = Directions.None)
    {
        Edges = edges;
        Exits = exits;
        Thickness = thickness;
    }

    private bool IsWall(int x, int y)
    {
        int midX = WIDTH / 2;
        int midY = HEIGHT / 2;
        int maxX = WIDTH - 1;
        int maxY = HEIGHT - 1;

        bool isEdge = (
            (x < Thickness && Edges.West())
            || (x > maxX - Thickness && Edges.East())
            || (y < Thickness && Edges.North())
            || (y > maxY - Thickness && Edges.South())
            );

        bool exitNorth = (
                    Exits.North()
                    && y < Thickness
                    && (x == midX || x == midX - 1)
                );

        bool exitSouth = (
                    Exits.South()
                    && y > maxY - Thickness
                    && (x == midX || x == midX - 1)
                );

        bool exitWest = (
                    Exits.West()
                    && x < Thickness
                    && (y == midY || y == midY - 1)
                );

        bool exitEast = (
                    Exits.East()
                    && x > maxX - Thickness
                    && (y == midY || y == midY - 1)
                );

        bool isExit = exitNorth || exitSouth || exitWest || exitEast;

        return isEdge && !isExit;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                subTileGrid[x + i, y + j] =
                    IsWall(i, j)
                    ? new WallSubTile()
                    : new FloorSubTile();
            }

        subTileGrid[x + HalfWidth, y + HalfHeight].Objects = Objects;
    }
}
