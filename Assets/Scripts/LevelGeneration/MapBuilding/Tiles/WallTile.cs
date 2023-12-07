using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WallTile : ATile
{
    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        for (int i = x; i < x + WIDTH; i++)
            for (int j = y; j < y + HEIGHT; j++)
                subTileGrid[i, j] = new WallSubTile();
    }
}
