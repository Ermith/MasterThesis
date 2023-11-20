using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Unity.VisualScripting;

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

                superTileGrid[x, y] = new Hallway(_superWidth, _superHeight, exits);
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
                    tile.Exits |= exits;
                else
                    superTileGrid[x, y] = new Hallway(_superWidth, _superHeight, exits);
            }
        }
        
        foreach ((var vertex, (int x, int y)) in _graphDrawing.VertexPositions)
        {
            var north = TryGetTile(x, y - 1, superTileGrid);
            var south = TryGetTile(x, y + 1, superTileGrid);
            var east = TryGetTile(x + 1, y, superTileGrid);
            var west = TryGetTile(x - 1, y, superTileGrid);

            Directions exits = Directions.None;
            if (north != null && north is Hallway nHallway && nHallway.Exits.South()) exits |= Directions.North;
            if (south != null && south is Hallway sHallway && sHallway.Exits.North()) exits |= Directions.South;
            if (east != null && east is Hallway eHallway && eHallway.Exits.West()) exits |= Directions.East;
            if (west != null && west is Hallway hallway && hallway.Exits.East()) exits |= Directions.West;

            superTileGrid[x, y] = new Room(_superWidth, _superHeight, exits);
            superTileGrid[x, y].Locks = vertex.GetLocks().ToList();
            superTileGrid[x, y].Keys = vertex.GetKeys().ToList();
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
                    enemies.Add(enemyParams);
                }

        enemiesParams = enemies.ToArray();
        return tileGrid;
    }
}