using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Unity.VisualScripting;

using URandom = UnityEngine.Random;

class MapBuilder
{
    private GraphDrawing<BaseVertex> _graphDrawing;
    private int _superWidth, _superHeight;
    private int _width, _height;

    public MapBuilder(GraphDrawing<BaseVertex> graphDrawing, int superWidth, int superHeight)
    {
        _graphDrawing = graphDrawing;
        _superWidth = superWidth;
        _superHeight = superHeight;
        _width = _graphDrawing.MaximumX + 1;
        _height = _graphDrawing.MaximumY + 1;
    }

    public ASubTile[,] SubTileGrid(ATile[,] tileGrid)
    {
        int width = _width * _superWidth;
        int height = _height * _superHeight;
        var subTileGrid = new ASubTile[width * ATile.WIDTH, height * ATile.HEIGHT];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (tileGrid[x, y] != null)
                    tileGrid[x, y].BuildSubTiles(x * ATile.WIDTH, y * ATile.HEIGHT, subTileGrid);


        return subTileGrid;
    }

    private Tile TryGetTile<Tile>(int x, int y, Tile[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        if (x < 0 || y < 0) return default;
        if (x >= width || y >= height) return default;

        return grid[x, y];
    }

    public ASuperTile[,] SuperTileGrid()
    {
        var superTileGrid = new ASuperTile[_width, _height];

        foreach ((int xFrom, int xTo, int y) in _graphDrawing.HorizontalLines)
            for (int x = xFrom; x <= xTo; x++)
            {
                Directions exits;
                if (x == xFrom) exits = Directions.East;
                else if (x == xTo) exits = Directions.West;
                else exits = Directions.East | Directions.West;

                ASuperTile tile;
                float t = URandom.Range(0f, 1f);
                if (t > 0.75f)
                    tile = new HallwayWithRooms(_superWidth, _superHeight, exits);
                else if (t > 0.50f)
                    tile = new WideHallway(_superWidth, _superHeight, exits);
                else if (t > 0.25)
                    tile = new FilledRoom(_superWidth, _superHeight, false, exits);
                else
                    tile = new Hallway(_superWidth, _superHeight, exits);

                superTileGrid[x, y] = tile;

                //if (tile is Hallway)
                    tile.Locks.Add(new EnemyLock());
            }

        foreach ((int x, int yFrom, int yTo) in _graphDrawing.VerticalLines)
        {
            for (int y = yFrom; y <= yTo; y++)
            {
                var tile = superTileGrid[x, y];

                Directions exits;
                if (y == yFrom) exits = Directions.South;
                else if (y == yTo) exits = Directions.North;
                else exits = Directions.North | Directions.South;

                if (tile != null)
                {
                    tile.Exits |= exits;
                    continue;
                }

                float t = URandom.Range(0f, 1f);
                if (t > 0.8f)
                    tile = new HallwayWithRooms(_superWidth, _superHeight, exits);
                else if (t > 0.50f)
                    tile = new WideHallway(_superWidth, _superHeight, exits);
                else if (t > 0.25f)
                    tile = new FilledRoom(_superWidth, _superHeight, false, exits);
                else
                    tile = new Hallway(_superWidth, _superHeight, exits);

                superTileGrid[x, y] = tile;
                tile.Locks.Add(new EnemyLock());
            }
        }
        
        foreach ((var vertex, (int x, int y)) in _graphDrawing.VertexPositions)
        {
            var north = TryGetTile(x, y - 1, superTileGrid);
            var south = TryGetTile(x, y + 1, superTileGrid);
            var east = TryGetTile(x + 1, y, superTileGrid);
            var west = TryGetTile(x - 1, y, superTileGrid);

            Directions exits = Directions.None;
            if (north != null && north is ASuperTile nTile && nTile.Exits.South()) exits |= Directions.North;
            if (south != null && south is ASuperTile sTile && sTile.Exits.North()) exits |= Directions.South;
            if (east != null && east is ASuperTile eTile && eTile.Exits.West()) exits |= Directions.East;
            if (west != null && west is ASuperTile wTile && wTile.Exits.East()) exits |= Directions.West;

            ASuperTile tile;
            if (URandom.Range(0f, 1f) > 0.4f)
                tile = new FilledRoom(_superWidth, _superHeight, true, exits);
            else
                tile = new Room(_superWidth, _superHeight, exits);

            superTileGrid[x, y] = tile;
            superTileGrid[x, y].Locks = vertex.GetLocks().ToList();
            superTileGrid[x, y].Keys = vertex.GetKeys().ToList();

            //if (tile is FilledRoom room)
                tile.Locks.Add(new EnemyLock());
        }

        return superTileGrid;
    }

    public ATile[,] TileGrid(ASuperTile[,] superTileGrid, out IEnumerable<EnemyParams> enemiesParams)
    {
        int width = _width * _superWidth;
        int height = _height * _superHeight;
        var tileGrid = new ATile[width, height];
        List<EnemyParams> enemies = new();

        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                if (superTileGrid[x, y] != null)
                {
                    var enemyParams = superTileGrid[x, y].BuildTiles(x * _superWidth, y * _superHeight, tileGrid);

                    foreach (var enemy in enemyParams)
                        enemies.Add(enemy);
                }

        enemiesParams = enemies.ToArray();
        return tileGrid;
    }
}