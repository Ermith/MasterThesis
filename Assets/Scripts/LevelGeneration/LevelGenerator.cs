using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class LevelGenerator : MonoBehaviour
{
    public int SuperWidth = 3;
    public int SuperHeight = 3;
    public GameObject WallBlueprint;
    public GameObject FloorBlueprint;
    public EnemyController EnemyBlueprint;

    IGraph<BaseVertex> _graph;
    GraphGenerator _graphGenerator;
    GraphDrawer<BaseVertex> _graphDrawer;
    MapBuilder<BaseVertex> _mapBuilder;

    // Intermediate Results
    public GraphDrawing<BaseVertex> GraphDrawing { get; private set; }
    public ASuperTile[,] SuperTileGrid;
    public ATile[,] TileGrid;
    public ASubTile[,] SubTileGrid;
    private void Awake()
    {
        _graph = new UndirectedAdjecencyGraph<BaseVertex>();
        _graphGenerator = new GraphGenerator(_graph);
        _graphGenerator.Generate();

        _graphDrawer = new GraphDrawer<BaseVertex>(_graph);
        GraphDrawing = _graphDrawer.Draw();

        _mapBuilder = new MapBuilder<BaseVertex>(GraphDrawing, SuperWidth, SuperHeight);
        SuperTileGrid = _mapBuilder.SuperTileGrid();
        TileGrid = _mapBuilder.TileGrid(SuperTileGrid, out IEnumerable<EnemyParams> enemies);
        SubTileGrid = _mapBuilder.SubTileGrid(TileGrid);

        ASubTile.Register<Wall>(WallBlueprint);
        ASubTile.Register<Floor>(FloorBlueprint);

        GameObject level = new("Level");
        GameObject geometry = new("Gemetry");
        Vector3 offset = new(100, 0, 100);
        geometry.transform.parent = level.transform;

        for (int x = 0; x < SubTileGrid.GetLength(0); x++)
            for (int y = 0; y < SubTileGrid.GetLength(1); y++)
                if (SubTileGrid[x, y] != null)
                {
                    GameObject obj = SubTileGrid[x, y].SpawnObject(x, y);
                    obj.transform.parent = geometry.transform;
                }

        float scale = 3;
        geometry.transform.localScale *= scale;
        foreach (EnemyParams enemy in enemies)
        {
            (int spawnX, int spawnZ) = enemy.Spawn;
            Vector3 spawn = new(spawnX, 0, spawnZ);
            spawn *= scale;

            List<Vector3> patrol = new();
            foreach ((int x, int z) in enemy.Patrol)
            {
                patrol.Add(new Vector3(x, 0, z) * scale + offset);
            }

            Instantiate(EnemyBlueprint, spawn, Quaternion.identity, level.transform).Patrol(patrol.ToArray());
        }

        // Just offset it for now
        level.transform.position = offset;
    }

    private void Start()
    {

    }
}