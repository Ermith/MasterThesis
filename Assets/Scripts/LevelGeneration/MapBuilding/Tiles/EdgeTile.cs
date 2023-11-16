using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EdgeTile : ATile
{
    public Directions Edges { get; set; }
    public EdgeTile(Directions edges)
    {
        Edges = edges;
    }

    private bool IsEdge(int x, int y)
    {
        return (
            (x == 0 && Edges.West())
            || (x == WIDTH - 1 && Edges.East())
            || (y == 0 && Edges.North())
            || (y == HEIGHT - 1 && Edges.South()));
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                subTileGrid[x + i, y + j] =
                    IsEdge(i, j) 
                    ? new Wall()
                    : new Floor();
            }
    }
}
