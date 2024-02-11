using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EmptyTile : ATile
{
    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                FloorSubTile floor = new();
                subTileGrid[x + i, y + j] = floor;
            }

        subTileGrid[x + HalfWidth, y + HalfHeight].Objects = Objects;
    }
}