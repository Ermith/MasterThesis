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
    public GameObject DoorBlueprint;
    public GameObject KeyBlueprint;
    public EnemyController EnemyBlueprint;

    IGraph<BaseVertex> _graph;
    GraphGenerator _graphGenerator;
    GraphDrawer<BaseVertex> _graphDrawer;
    MapBuilder _mapBuilder;

    // Intermediate Results
    public IGraph<BaseVertex> Graph { get; private set; }
    public GraphDrawing<BaseVertex> GraphDrawing { get; private set; }
    public Dictionary<Key, BaseVertex> KeyVertexMapping;
    public Dictionary<Lock, BaseVertex> LockVertexMapping;
    public ASuperTile[,] SuperTileGrid;
    public ATile[,] TileGrid;
    public ASubTile[,] SubTileGrid;
    private void Awake()
    {

        DoorKey.KeyBlueprint = KeyBlueprint;
        _graph = new UndirectedAdjecencyGraph<BaseVertex>();
        _graphGenerator = new GraphGenerator(_graph);
        _graphGenerator.Generate();
        Graph = _graph;
        KeyVertexMapping = _graphGenerator.KeyMapping;
        LockVertexMapping = _graphGenerator.LockMapping;

        _graphDrawer = new GraphDrawer<BaseVertex>(_graph);
        GraphDrawing = _graphDrawer.Draw();

        _mapBuilder = new MapBuilder(GraphDrawing, SuperWidth, SuperHeight);
        SuperTileGrid = _mapBuilder.SuperTileGrid();
        TileGrid = _mapBuilder.TileGrid(SuperTileGrid, out IEnumerable<EnemyParams> enemies);
        SubTileGrid = _mapBuilder.SubTileGrid(TileGrid);

        ASubTile.Register<WallSubTile>((ASubTile st) => Instantiate(WallBlueprint));
        ASubTile.Register<FloorSubTile>((ASubTile st) => Instantiate(FloorBlueprint));
        ASubTile.Register<DoorSubTile>((ASubTile st) =>
        {
            var door = st as DoorSubTile;
            GameObject doorObject = Instantiate(DoorBlueprint);
            doorObject.GetComponent<Door>().DoorLock = door.DoorLock;
            return doorObject;
        });


        GameObject level = new("Level");
        GameObject geometry = new("Geometry");
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
            (int spawnX, int spawnZ) = enemy.Patrol.ToList()[enemy.Spawn];
            Vector3 spawn = new(spawnX, 0, spawnZ);
            spawn *= scale;

            List<Vector3> patrol = new();
            foreach ((int x, int z) in enemy.Patrol)
            {
                patrol.Add(new Vector3(x, 0, z) * scale + offset);
            }

            Instantiate(EnemyBlueprint, spawn, Quaternion.identity, level.transform).Patrol(patrol.ToArray(), enemy.Spawn);
        }

        // Just offset it for now
        level.transform.position = offset;
    }

    private void Start()
    {

    }
}