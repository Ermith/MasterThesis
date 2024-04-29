using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The class responsible for whole generation process on Start() function. 
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    public int SuperWidth = 3;
    public int SuperHeight = 3;

    // Blueprints
    public GameObject WallBlueprint;
    public GameObject FloorBlueprint;
    public GameObject DoorBlueprint;
    public GameObject WallOfLightBlueprint;
    public GameObject HiddenDoorBlueprint;
    public GameObject KeyBlueprint;
    public GameObject TrapKitBlueprint;
    public GameObject CamoBlueprint;
    public GameObject SecurityCameraBlueprint;
    public GameObject PowerSourceBlueprint;
    public GameObject TrapBlueprint;
    public GameObject SoundTrapBlueprint;
    public GameObject RefugeBlueprint;
    public GameObject VictoryTrigger;
    public GameObject ObjectiveBlueprint;
    public GameObject StairsBlueprint;
    public EnemyController EnemyBlueprint;

    public GameObject Player;
    public Map Map;

    // Rendering
    public Material FloorMaterial;
    public Material WallMaterial;
    public Mesh FloorMesh;
    public Mesh WallMesh;

    private GridGraph _graph;
    private GridGraphGenerator _graphGenerator;
    private GraphGridDrawer _graphDrawer;
    private MapBuilder _mapBuilder;

    private int _floorHeight = 3;

    private List<Matrix4x4>[] _floorMatrices;
    private List<Matrix4x4>[] _wallMatrices;
    private GameObject _geometry;
    private GameObject[] _floors;
    private int _activeFloor;
    private bool _wide = false;

    // Intermediate Results
    public GraphDrawing<GridVertex> GraphDrawing { get; private set; }
    public Dictionary<IKey, GridVertex> KeyVertexMapping;
    public Dictionary<ILock, GridVertex> LockVertexMapping;
    public ASuperTile[,] SuperTileGrid;
    public ATile[,] TileGrid;
    public ASubTile[,] SubTileGrid;

    private void Start()
    {
        RegisterBlueprints();

        Debug.Log("GENERATING GRAPH");
        _graph = new GridGraph();
        _graphGenerator = new GridGraphGenerator(_graph);
        _graphGenerator.Generate();
        KeyVertexMapping = _graphGenerator.KeyMapping;
        LockVertexMapping = _graphGenerator.LockMapping;

        Debug.Log("DRAWING THE GRAPH");
        _graphDrawer = new GraphGridDrawer(_graph);
        GraphDrawing = _graphDrawer.Draw(_graphGenerator.GetStartVertex(), _graphGenerator.GetEndVertex());

        Debug.Log("CREATING SUPERTILES");
        _mapBuilder = new MapBuilder(GraphDrawing, SuperWidth, SuperHeight);
        var superTileGrids = _mapBuilder.SuperTileGrid();

        Debug.Log("CREATING TILES");
        var tileGrids = _mapBuilder.TileGrid(superTileGrids, out IEnumerable<EnemyParams> enemies);

        Debug.Log("Creating SUBTILES");
        var subTileGrids = _mapBuilder.SubTileGrid(tileGrids);

        Debug.Log("Spawning Objects");
        GameObject level = SpawnObjects(subTileGrids);

        Debug.Log("Spawning Enemies");
        SpawnEnemies(enemies);

        Debug.Log("Spawning Player");
        var playerSpawn = _mapBuilder.GetSpawnPosition();
        playerSpawn.y *= _floorHeight;
        // Spawn player at the correct position
        // Needs to be 1 frame delayed because of a bug, when setting position works only occasionally
        StartCoroutine(DelayedSpawn(playerSpawn));

        Debug.Log("Creating a Map");
        Map.Drawing = GraphDrawing;
        Map.CreateMap();
        level.transform.parent = transform;

        Debug.Log("Building Navmesh");
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private IEnumerator DelayedSpawn(Vector3 spawnPosition)
    {
        yield return null;

        Debug.Log($"REPOSITIONING THE PLAYER {spawnPosition}");
        Player.transform.position = spawnPosition;
    }

    void Update()
    {
        // Instanced Rendering of Floors and Walls due to their high quantity

        RenderParams floorParams = new RenderParams(FloorMaterial);
        floorParams.receiveShadows = true;
        floorParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        RenderParams wallParams = new RenderParams(WallMaterial);
        wallParams.receiveShadows = true;
        wallParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        if (!_wide)
        {
            RenderInstanced(floorParams, FloorMesh, _floorMatrices[_activeFloor]);
            RenderInstanced(wallParams, WallMesh, _wallMatrices[_activeFloor]);
        } else
        {
            for (int i = 0; i < _floorMatrices.Length; ++i)
                if (i >= _activeFloor - 1 && i <= _activeFloor + 1)
                    RenderInstanced(floorParams, FloorMesh, _floorMatrices[i]);


            for (int i = 0; i < _wallMatrices.Length; ++i)
                if (i >= _activeFloor - 1 && i <= _activeFloor + 1)
                    RenderInstanced(wallParams, WallMesh, _wallMatrices[i]);
        }
    }

    /// <summary>
    /// Instanced rendering takex max 1023 objects. This functions splits the array before sending it for rendering.
    /// </summary>
    /// <param name="rp"></param>
    /// <param name="mesh"></param>
    /// <param name="ms"></param>
    public void RenderInstanced(RenderParams rp, Mesh mesh, IEnumerable<Matrix4x4> ms)
    {
        List<Matrix4x4> buffer = new(1023);
        foreach (Matrix4x4 m in ms)
        {
            buffer.Add(m);
            if (buffer.Count == 1023)
            {
                Graphics.RenderMeshInstanced(rp, mesh, 0, buffer);
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
            Graphics.RenderMeshInstanced(rp, mesh, 0, buffer);
    }

    /// <summary>
    /// Transforms world position into a posion in the tilemap.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public (int x, int y, int floor) GridCoordinates(Vector3 position)
    {
        Vector3 relative = position - _geometry.transform.position;
        int x = (int)Mathf.Floor(relative.x / (SuperWidth * ATile.WIDTH));
        int y = (int)Mathf.Floor(relative.z / (SuperHeight * ATile.HEIGHT));
        int floor = (int)Mathf.Floor(relative.y / _floorHeight);

        return (x, y, floor);
    }

    /// <summary>
    /// Upper floors and roofs are not rendered. This is used for instance of being in top-down view.
    /// </summary>
    /// <param name="floor"></param>
    public void HighlightFloor(int floor)
    {
        _activeFloor = floor;
        _wide = false;
        for (int i = 0; i < _floors.Length; i++)
        {
            foreach (var m in _floors[i].GetComponentsInChildren<MeshRenderer>())
                m.enabled = i == floor;
        }
    }

    /// <summary>
    /// Even when no floor is highlighted, render a maximum of three floors.
    /// </summary>
    /// <param name="floor"></param>
    public void UnHilightFloors(int floor)
    {
        for (int i = 0; i < _floors.Length; i++)
        {
            foreach (var m in _floors[i].GetComponentsInChildren<MeshRenderer>())
                m.enabled = i >= floor - 1 && i <= floor + 1;
        }
        _activeFloor = floor;
        _wide = true;
    }

    /// <summary>
    /// Uses the <see cref="BlueprintManager"/> class to map subtiles and keys C# classes to Unity spawnable classes.
    /// </summary>
    private void RegisterBlueprints()
    {
        // Keys
        BlueprintManager.Register<DoorKey>(() => Instantiate(KeyBlueprint));
        BlueprintManager.Register<PowerSourceKey>(() => Instantiate(PowerSourceBlueprint));
        BlueprintManager.Register<TrapDisarmingKitKey>(() => Instantiate(TrapKitBlueprint));
        BlueprintManager.Register<InvisibiltyCamoKey>(() => Instantiate(CamoBlueprint));
        BlueprintManager.Register<SideObjectiveKey>(() => Instantiate(ObjectiveBlueprint));

        // Locks
        BlueprintManager.Register<SecurityCameraLock>(() => Instantiate(SecurityCameraBlueprint));
        BlueprintManager.Register<DeathTrapLock>(() => Instantiate(TrapBlueprint));
        BlueprintManager.Register<SoundTrapLock>(() => Instantiate(SoundTrapBlueprint));

        // Subtiles
        BlueprintManager.Register<DoorSubTile>(() => Instantiate(DoorBlueprint));
        BlueprintManager.Register<FloorSubTile>(() => Instantiate(FloorBlueprint));
        BlueprintManager.Register<HiddenDoorSubTile>(() => Instantiate(HiddenDoorBlueprint));
        BlueprintManager.Register<NoneSubTile>(() => new GameObject());
        BlueprintManager.Register<RefugeSubTile>(() => Instantiate(RefugeBlueprint));
        BlueprintManager.Register<WallOfLightSubTile>(() => Instantiate(WallOfLightBlueprint));
        BlueprintManager.Register<WallSubTile>(() => Instantiate(WallBlueprint));

        // Stairs
        BlueprintManager.Register<StairwayRoom>(() => Instantiate(StairsBlueprint));
    }

    /// <summary>
    /// Spawns the objects of Subtiles and sub objects they contain.
    /// </summary>
    /// <param name="subTileGrids"></param>
    /// <returns></returns>
    private GameObject SpawnObjects(List<ASubTile[,]> subTileGrids)
    {
        GameObject level = new("Level");
        GameObject geometry = new("Geometry");
        _geometry = geometry;
        geometry.transform.parent = level.transform;

        // Roof
        int w = subTileGrids[subTileGrids.Count - 1].GetLength(0);
        int h = subTileGrids[subTileGrids.Count - 1].GetLength(1);
        subTileGrids.Add(new ASubTile[w, h]);

        // Params for instanced rendering of floor and walls.
        _floors = new GameObject[subTileGrids.Count];
        _floorMatrices = new List<Matrix4x4>[subTileGrids.Count];
        for (int i = 0; i < _floorMatrices.Length; i++)
            _floorMatrices[i] = new List<Matrix4x4>();

        _wallMatrices = new List<Matrix4x4>[subTileGrids.Count];
        for (int i = 0; i < _wallMatrices.Length; i++)
            _wallMatrices[i] = new List<Matrix4x4>();

        // Spawn the actual objects
        for (int floor = subTileGrids.Count - 1; floor >= 0; floor--)
        {
            // Objects are spawned under floor objects.
            var grid = subTileGrids[floor];
            var floorGameObject = new GameObject($"Floor {floor}");
            floorGameObject.transform.parent = geometry.transform;
            _floors[floor] = floorGameObject;

            for (int col = 0; col < grid.GetLength(0); col++)
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    // Spawn the roof for tile beneath.
                    if (grid[col, row] == null)
                    {
                        if (floor > 0 && subTileGrids[floor - 1][col, row] != null)
                        {
                            grid[col, row] = new FloorSubTile();
                        } else
                            continue;
                    }

                    // Spawn the subtile object. It contains subobjects as well.
                    GameObject obj = grid[col, row].Spawn(col, row, floor * _floorHeight);
                    obj.transform.position = obj.transform.position.Added(y: 0.1f * floor);

                    // Add for instanced rendering if floor or a wall
                    if (grid[col, row] is FloorSubTile)
                    {
                        var m = new Matrix4x4();
                        m.SetTRS(obj.transform.position.Added(y: -0.05f), obj.transform.rotation, obj.transform.localScale.Set(y: 0.1f));
                        _floorMatrices[floor].Add(m);
                    }

                    if (grid[col, row] is WallSubTile)
                    {
                        var m = new Matrix4x4();
                        m.SetTRS(obj.transform.position.Added(y: 1.5f), obj.transform.rotation, obj.transform.localScale.Set(y: 3f));
                        _wallMatrices[floor].Add(m);
                    }

                    obj.transform.parent = floorGameObject.transform;
                }
        }

        // Finally, spawn the victory trigger.
        var pos = _mapBuilder.GetEndPosition();
        int victoryFloor = (int)pos.y;
        pos.y *= _floorHeight;
        var victory = GameObject.Instantiate(VictoryTrigger);
        victory.transform.position = pos;
        victory.transform.parent = _floors[victoryFloor].transform;

        return level;
    }

    /// <summary>
    /// Spawns the enemies unde _floors[] objects.
    /// </summary>
    /// <param name="enemies"></param>
    private void SpawnEnemies(IEnumerable<EnemyParams> enemies)
    {
        if (GenerationSettings.DangerEnemies)
            foreach (EnemyParams enemy in enemies)
            {
                (int spawnX, int spawnZ) = enemy.Spawn;
                Vector3 spawn = new(spawnX, (enemy.Floor + 0.1f) * _floorHeight, spawnZ);

                var enemyInstance = Instantiate(
                    EnemyBlueprint,
                    spawn,
                    Quaternion.identity);

                enemyInstance.transform.parent = _floors[enemy.Floor].transform;

                if (enemy.Behaviour == Behaviour.Patroling)
                {
                    List<Vector3> patrol = new();
                    foreach ((int x, int z) in enemy.Patrol)
                    {
                        patrol.Add(new Vector3(x, (enemy.Floor + 0.1f) * _floorHeight, z));
                    }

                    enemyInstance.Patrol(patrol.ToArray(), enemy.PatrolIndex, enemy.Retrace);
                }

                if (enemy.Behaviour == Behaviour.Guarding)
                {
                    enemyInstance.Guard(spawn, Vector3.right);
                    enemyInstance.LookAt(spawn);
                }

                if (enemy.Behaviour == Behaviour.Sleeping)
                    enemyInstance.Sleep(spawn);
            }
    }
}