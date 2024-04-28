using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using URandom = UnityEngine.Random;

class MapBuilder
{
    private GraphDrawing<GridVertex> _graphDrawing;
    private int _superWidth, _superHeight;
    private int _width, _height;

    public MapBuilder(GraphDrawing<GridVertex> graphDrawing, int superWidth, int superHeight)
    {
        _graphDrawing = graphDrawing;
        _superWidth = superWidth;
        _superHeight = superHeight;
        _width = _graphDrawing.MaximumX + 1;
        _height = _graphDrawing.MaximumY + 1;
    }

    public List<ASubTile[,]> SubTileGrid(List<ATile[,]> tileGrids)
    {
        int width = _width * _superWidth;
        int height = _height * _superHeight;
        List<ASubTile[,]> subGrids = new();

        for (int i = 0; i < tileGrids.Count; i++)
        {
            var subGrid = new ASubTile[width * ATile.WIDTH, height * ATile.HEIGHT];
            var tileGrid = tileGrids[i];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (tileGrid[x, y] != null)
                        tileGrid[x, y].BuildSubTiles(x * ATile.WIDTH, y * ATile.HEIGHT, subGrid);

            subGrids.Add(subGrid);
        }

        return subGrids;
    }

    private Tile TryGetTile<Tile>(int x, int y, Tile[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        if (x < 0 || y < 0) return default;
        if (x >= width || y >= height) return default;

        return grid[x, y];
    }

    public List<ASuperTile[,]> SuperTileGrid()
    {
        var floorTransitions = new List<(int x, int y, int zFrom, int zTo)>();
        var superTileGrids = new List<ASuperTile[,]>();
        for (int i = 0; i < _graphDrawing.MaximumZ + 1; i++)
            superTileGrids.Add(new ASuperTile[_width, _height]);

        foreach ((var vertex, (int x, int y, int z)) in _graphDrawing.VertexPositions)
        {
            ASuperTile tile;

            float random = URandom.value;
            if (vertex.Top || vertex.Bottom) tile = new StairwayRoom(_superWidth, _superHeight, z, vertex.Exits, up: vertex.Top, down: vertex.Bottom, reveresed: z % 2 == 0);
            else if (vertex.Hallway && vertex.GetKeys().ToArray().Length > 0) tile = new HallwayWithRooms(_superWidth, _superHeight, z, vertex.Exits);
            else if (URandom.value > 0.5f) tile = new RecursiveRoom(_superWidth, _superHeight, z, exits: vertex.Exits);
            else tile = new Room(_superWidth, _superHeight, z, vertex.Exits);

            tile.Locks = vertex.GetLocks().ToList();
            tile.Keys = vertex.GetKeys().ToList();

            superTileGrids[z][x, y] = tile;
        }

        foreach ((IEdge<GridVertex> e, List<(int x, int y, int z)> positions) in _graphDrawing.EdgePositions)
        {
            var firstPosition = positions[0];
            var lastPosition = positions[positions.Count - 1];

            // Interfloor connection
            if (firstPosition.z != lastPosition.z)
            {
                floorTransitions.Add((firstPosition.x, firstPosition.y, firstPosition.z, lastPosition.z));
                continue;
            }

            List<(int, int)> path = new();
            path.Add((firstPosition.x, firstPosition.y));
            var grid = superTileGrids[firstPosition.z];

            for (int i = 1; i < positions.Count; i++)
            {
                (int fromX, int fromY, int _) = positions[i - 1];
                (int toX, int toY, int _) = positions[i];

                path.AddRange(Utils.GetShortPath(fromX, fromY, toX, toY).Skip(1));
            }

            for (int i = 1; i < path.Count - 1; i++)
            {
                (int fromX, int fromY) = path[i - 1];
                (int toX, int toY) = path[i];
                (int nextX, int nextY) = path[i + 1];

                Directions exits = Directions.None;
                if (fromX < toX) exits |= Directions.West;
                if (fromX > toX) exits |= Directions.East;
                if (fromY < toY) exits |= Directions.South;
                if (fromY > toY) exits |= Directions.North;

                if (nextX < toX) exits |= Directions.West;
                if (nextX > toX) exits |= Directions.East;
                if (nextY < toY) exits |= Directions.South;
                if (nextY > toY) exits |= Directions.North;

                ASuperTile tile;
                float random = URandom.value;
                if (random > 0.66f)
                    tile = new Hallway(_superWidth, _superHeight, firstPosition.z, exits);
                else if (random > 0.33f)
                    tile = new WideHallway(_superWidth, _superHeight, firstPosition.z, exits);
                else
                    tile = new Roundabout(_superHeight, _superHeight, firstPosition.z, exits: exits);

                if (URandom.value > 0.5f)
                    tile.Locks.Add(new EnemyLock());
                
                grid[toX, toY] = tile;
            }
        }

        foreach ((int x, int y, int zFrom, int zTo) in floorTransitions)
        {
            int zMin = Math.Min(zFrom, zTo);
            int zMax = Math.Max(zFrom, zTo);

            for (int z = zMin + 1; z < zMax; z++)
            {
                superTileGrids[z][x, y] = new StairwayRoom(_superWidth, _superHeight, z, up: true, down: true, reveresed: z % 2 == 0);

                var v = new GridVertex();
                v.Bottom = true;
                v.Top = true;
                _graphDrawing.VertexPositions.Add(v, (x, y, z));
            }
        }

        Func<int, int, Directions, ASuperTile[,], bool> isHallway = (int x, int y, Directions dir, ASuperTile[,] grid) =>
        {
            (int dx, int dy) = dir.ToCoords();
            x += dx;
            y += dy;

            if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
                return false;

            return grid[x, y] is Hallway;
        };

        foreach ((var vertex, (int x, int y, int z)) in _graphDrawing.VertexPositions)
        {
            if (isHallway(x, y, Directions.North, superTileGrids[z]) || y % 2 == 0)
                superTileGrids[z][x, y].HasDefaultDoor |= Directions.North;

            if (isHallway(x, y, Directions.South, superTileGrids[z]) || y % 2 == 0)
                superTileGrids[z][x, y].HasDefaultDoor |= Directions.South;

            if (isHallway(x, y, Directions.East, superTileGrids[z]) || x % 2 == 0)
                superTileGrids[z][x, y].HasDefaultDoor |= Directions.East;

            if (isHallway(x, y, Directions.West, superTileGrids[z]) || x % 2 == 0)
                superTileGrids[z][x, y].HasDefaultDoor |= Directions.West;
        }

        return superTileGrids;
    }

    public List<ATile[,]> TileGrid(List<ASuperTile[,]> superTileGrids, out IEnumerable<EnemyParams> enemiesParams)
    {
        int width = _width * _superWidth;
        int height = _height * _superHeight;

        List<ATile[,]> tileGrids = new();
        List<EnemyParams> enemies = new();

        for (int i = 0; i < superTileGrids.Count; i++)
        {
            var grid = new ATile[width, height];
            var superGrid = superTileGrids[i];

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    if (superGrid[x, y] != null)
                    {
                        var tileEnemies = superGrid[x, y].BuildTiles(x * _superWidth, y * _superHeight, grid);

                        foreach (var enemy in tileEnemies)
                            enemies.Add(enemy);
                    }

            tileGrids.Add(grid);
        }

        enemiesParams = enemies.ToArray();
        return tileGrids;
    }

    public Vector3 GetSpawnPosition()
    {
        (int x, int y, int z) = _graphDrawing.VertexPositions[_graphDrawing.StartPosition];
        return new Vector3(
            (x + 0.5f) * _superWidth * ATile.WIDTH,
            z,
            (y + 0.5f) * _superHeight * ATile.HEIGHT
        );
    }

    public Vector3 GetEndPosition()
    {
        (int x, int y, int z) = _graphDrawing.VertexPositions[_graphDrawing.EndPosition];
        return new Vector3(
            (x + 0.5f) * _superWidth * ATile.WIDTH,
            z,
            (y + 0.5f) * _superHeight * ATile.HEIGHT
        );
    }
}