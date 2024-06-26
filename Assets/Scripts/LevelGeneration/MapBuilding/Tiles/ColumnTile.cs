﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

/// <summary>
/// Tile with walls in the middle and based on directions.
/// </summary>
public class ColumnTile : ATile
{
    Directions Directions { get; set; }
    bool Mid;
    public ColumnTile(Directions directions, bool mid = true)
    {
        Directions = directions;
        Mid = mid;
    }

    public override void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid)
    {
        int xmid = x + WIDTH / 2;
        int ymid = y + HEIGHT / 2;

        if (Directions.North())
            for (int i = y; i < ymid; i++)
                subTileGrid[xmid, i] = new WallSubTile();

        if (Directions.South())
            for (int i = ymid + 1; i < y + HEIGHT; i++)
                subTileGrid[xmid, i] = new WallSubTile();

        if (Directions.West())
            for (int i = x; i < xmid; i++)
                subTileGrid[i, ymid] = new WallSubTile();

        if (Directions.East())
            for (int i = xmid + 1; i < x + WIDTH; i++)
                subTileGrid[i, ymid] = new WallSubTile();
        
        if (Mid)
            subTileGrid[xmid, ymid] = new WallSubTile();

        for (int i = x; i < x + WIDTH; i++)
            for (int j = y; j < y + HEIGHT; j++)
                subTileGrid[i, j] ??= new FloorSubTile();
    }
}