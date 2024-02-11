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

    public EdgeTile(Directions edges, int thickness = 1,  Directions exits = Directions.None)
    {
        Edges = edges;
        Exits = exits;
        Thickness = thickness;
    }

    private bool IsWall(int x, int y)
    {
        return (
            (x < Thickness && Edges.West())
            || (x > WIDTH - 1 - Thickness && Edges.East())
            || (y < Thickness && Edges.North())
            || (y > HEIGHT - 1 - Thickness && Edges.South())
            )
            && !(
            (x == WIDTH / 2 && y < Thickness && Exits.North())
            || (x == WIDTH / 2 && y > HEIGHT - 1 - Thickness && Exits.South())
            || (x < Thickness && y == HEIGHT / 2 && Exits.West())
            || (x > WIDTH - 1 - Thickness && y == HEIGHT / 2 && Exits.East())
            );
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
