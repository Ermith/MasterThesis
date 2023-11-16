using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GraphDrawingWindow : EditorWindow
{
    private LevelGenerator _levelGenerator = null;

    private const string GRAPH = "Graph";
    private const string SUPER_TILES = "SuperTiles";
    private const string TILES = "Tiles";
    private const string SUB_TILES = "SubTiles";
    private string[] _states = new string[]
    {
        GRAPH, SUPER_TILES, TILES, SUB_TILES
    };
    private int _stateIndex = 0;

    private float scale = 1f;
    private Vector3 offset = Vector2.zero;

    [MenuItem("Window/LevelVisualizer")]
    public static void ShowWindow()
    {
        GetWindow<GraphDrawingWindow>("Graph");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.KeypadPlus)) scale += 0.1f;
        if (Input.GetKeyDown(KeyCode.KeypadMinus)) scale--;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        _levelGenerator = EditorGUILayout.ObjectField("LevelGenerator", _levelGenerator, typeof(LevelGenerator), true) as LevelGenerator;
        _stateIndex = EditorGUILayout.Popup(_stateIndex, _states);
        EditorGUILayout.EndHorizontal();
        if (_levelGenerator == null)
            return;

        var e = Event.current;
        if (e.isKey)
        {
            if (e.keyCode == KeyCode.KeypadPlus) { scale += 0.1f; }
            if (e.keyCode == KeyCode.KeypadMinus) { scale -= 0.1f; }
            Repaint();
        }

        if (e.isScrollWheel)
        {
            scale += -e.delta.y * 0.01f;
            Repaint();
        }

        if (e.isMouse && e.IsRightMouseButton())
        {

            offset += new Vector3(e.delta.x, e.delta.y);

            Repaint();
        }

        switch (_states[_stateIndex])
        {
            case GRAPH: DrawGraph(_levelGenerator.GraphDrawing); break;
            case SUPER_TILES: DrawSuperTiles(_levelGenerator.SuperTileGrid); break;
            case TILES: DrawTiles(_levelGenerator.TileGrid); break;
            case SUB_TILES: DrawSubTiles(_levelGenerator.SubTileGrid); break;
            default: break;
        }
    }

    private void DrawGraph(GraphDrawing<BaseVertex> drawParams)
    {
        int spacing = 50;
        int radius = (int)(20 * scale);

        foreach ((int xFrom, int xTo, int y) in drawParams.HorizontalLines)
            Handles.DrawLine(
                new Vector3(xFrom, y) * spacing * scale + offset,
                new Vector3(xTo, y) * spacing * scale + offset);

        foreach ((int x, int yFrom, int yTo) in drawParams.VerticalLines)
            Handles.DrawLine(
                new Vector3(x, yFrom) * spacing * scale + offset,
                new Vector3(x, yTo) * spacing * scale + offset);

        foreach ((var _, (int x, int y)) in drawParams.VertexPositions)
            Handles.DrawSolidDisc(
                new Vector3(x, y) * spacing * scale + offset,
                Vector3.back, radius);
    }

    private void DrawSuperTiles(ASuperTile[,] grid) { DrawGenericTileGrid(grid); }
    private void DrawTiles(ATile[,] grid) { DrawGenericTileGrid(grid); }
    private void DrawSubTiles(ASubTile[,] grid) { DrawGenericTileGrid(grid); }

    private void DrawGenericTileGrid<T>(T[,] grid)
    {
        int tileSize = 50;
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = 0; x <= width; x++)
            Handles.DrawLine(
                new Vector3(x * tileSize, 0) * scale + offset,
                new Vector3(x * tileSize, height * tileSize) * scale + offset);

        for (int y = 0; y <= height; y++)
            Handles.DrawLine(
                new Vector3(0, y * tileSize) * scale + offset,
                new Vector3(width * tileSize, y * tileSize) * scale + offset);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                T tile = grid[x, y];
                string text = tile == null ? "" : tile.ToString();

                Handles.Label(
                    new Vector3(x * tileSize, y * tileSize + tileSize / 2) * scale + offset,
                    text);
            }

    }
}
