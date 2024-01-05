using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RefugeEdgeTile : EdgeTile
{
    Directions Refuges;
    public RefugeEdgeTile(Directions refuges, Directions edges, Directions exits = Directions.None) : base(edges, exits)
    {
        Refuges = refuges;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        base.BuildSubTiles(x, y, subTileGrid);

        if (Refuges.North()) subTileGrid[x + 1, y] = new HalfRefugeSubTile(Directions.North);
        if (Refuges.South()) subTileGrid[x + 1, y + 2] = new HalfRefugeSubTile(Directions.South);
        if (Refuges.West()) subTileGrid[x, y + 1] = new HalfRefugeSubTile(Directions.West);
        if (Refuges.East()) subTileGrid[x + 2, y + 1] = new HalfRefugeSubTile(Directions.East);
    }
}