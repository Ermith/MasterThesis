using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EdgeTile : ATile
{
    public Directions Edges { get; set; }
    public Directions Exits { get; set; }
    public EdgeTile(Directions edges, Directions exits = Directions.None)
    {
        Edges = edges;
        Exits = exits;
    }

    private bool IsWall(int x, int y)
    {
        return (
            (x == 0 && Edges.West())
            || (x == WIDTH - 1 && Edges.East())
            || (y == 0 && Edges.North())
            || (y == HEIGHT - 1 && Edges.South())
            )
            && !(
            (x == WIDTH / 2 && y == 0 && Exits.North())
            || (x == WIDTH / 2 && y == HEIGHT - 1 && Exits.South())
            || (x == 0 && y == HEIGHT / 2 && Exits.West())
            || (x == WIDTH - 1 && y == HEIGHT / 2 && Exits.East())
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

        subTileGrid[x + 1, y + 1].Objects = Objects;
    }
}
