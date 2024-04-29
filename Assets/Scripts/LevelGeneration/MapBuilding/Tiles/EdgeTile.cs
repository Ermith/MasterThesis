using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Tile with walls on the sides based on given directions. Also can spawn empty spaces for exits.
/// </summary>
public class EdgeTile : ATile
{
    public Directions Edges { get; set; }
    public Directions Exits { get; set; }
    public int Thickness { get; private set; }
    public bool Floor;

    public EdgeTile(Directions edges, int thickness = 1, Directions exits = Directions.None, bool floor = true)
    {
        Edges = edges;
        Exits = exits;
        Thickness = thickness;
        Floor = floor;
    }

    /// <summary>
    /// Returns true if it's on the edge nad not in exit coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsWall(int x, int y)
    {
        int midX = WIDTH / 2;
        int midY = HEIGHT / 2;
        int maxX = WIDTH - 1;
        int maxY = HEIGHT - 1;

        bool isEdge = (
            (x < Thickness && Edges.West())
            || (x > maxX - Thickness && Edges.East())
            || (y < Thickness && Edges.South())
            || (y > maxY - Thickness && Edges.North())
            );

        bool exitNorth = (
                    Exits.South()
                    && y < Thickness
                    && (x == midX || x == midX - 1)
                );

        bool exitSouth = (
                    Exits.North()
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
                if (IsWall(i, j)) subTileGrid[x + i, y + j] = new WallSubTile();
                else if (Floor) subTileGrid[x + i, y + j] = new FloorSubTile();
                else subTileGrid[x + i, y + j] = new NoneSubTile();
            }

        subTileGrid[x + HalfWidth, y + HalfHeight].Objects = Objects;
    }
}
