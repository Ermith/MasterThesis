using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RefugeEdgeTile : EdgeTile
{
    Directions Refuges;
    public RefugeEdgeTile(Directions refuges, Directions edges, Directions exits = Directions.None, int thickness = 2) : base(edges, thickness, exits)
    {
        Refuges = refuges;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);

        if (Refuges.North())
        {
            int rx = x + HalfWidth;
            int ry = y + + HEIGHT - 1 - Thickness + 1;

            subTileGrid[rx, ry] = new RefugeSubTile();
            subTileGrid[rx - 1, ry] = new RefugeSubTile();
            //subTileGrid[rx + 1, ry] = new RefugeSubTile();
        }

        if (Refuges.South())
        {
            int rx = x + HalfWidth;
            int ry = y + Thickness - 1;

            subTileGrid[rx, ry] = new RefugeSubTile();
            subTileGrid[rx - 1, ry] = new RefugeSubTile();
            //subTileGrid[rx + 1, ry] = new RefugeSubTile();
        }

        if (Refuges.West())
        {
            int rx = x + Thickness - 1;
            int ry = y + HalfHeight;

            subTileGrid[rx, ry] = new RefugeSubTile();
            subTileGrid[rx, ry - 1] = new RefugeSubTile();
            //subTileGrid[rx, ry + 1] = new RefugeSubTile();
        }

        if (Refuges.East())
        {
            int rx = x + WIDTH - 1 - Thickness + 1;
            int ry = y + HalfHeight;

            subTileGrid[rx, ry] = new RefugeSubTile();
            subTileGrid[rx, ry - 1] = new RefugeSubTile();
            //subTileGrid[rx, ry + 1] = new RefugeSubTile();
        }
    }
}