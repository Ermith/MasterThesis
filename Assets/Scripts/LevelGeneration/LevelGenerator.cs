using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int SuperWidth = 3;
    public int SuperHeight = 3;
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
    public GameObject StairsBlueprint;
    public EnemyController EnemyBlueprint;
    public GameObject Player;

    public Map Map;

    public Material FloorMaterial;
    public Material WallMaterial;
    public Mesh FloorMesh;
    public Mesh WallMesh;

    GridGraph _graph;
    GraphGenerator _graphGenerator;
    GraphGridDrawer _graphDrawer;
    MapBuilder _mapBuilder;

    private bool _done = false;
    private int _floorHeight = 3;

    private List<Matrix4x4>[] _floorMatrices;
    private List<Matrix4x4>[] _wallMatrices;
    private GameObject _geometry;
    private GameObject[] _floors;
    private GameObject[] _walls;
    private int? _activeFloor;

    // Intermediate Results
    public GridGraph Graph { get; private set; }
    public GraphDrawing<GridVertex> GraphDrawing { get; private set; }
    public Dictionary<IKey, GridVertex> KeyVertexMapping;
    public Dictionary<ILock, GridVertex> LockVertexMapping;
    public ASuperTile[,] SuperTileGrid;
    public ATile[,] TileGrid;
    public ASubTile[,] SubTileGrid;
    private void Start()
    {
        //*/
        DoorKey.Blueprint = KeyBlueprint;
        SecurityCameraLock.Blueprint = SecurityCameraBlueprint;
        PowerSourceKey.Blueprint = PowerSourceBlueprint;
        TrapLock.Blueprint = TrapBlueprint;
        SoundTrapLock.Blueprint = SoundTrapBlueprint;
        StairwayRoom.Blueprint = StairsBlueprint;
        TrapDisarmingKit.Blueprint = TrapKitBlueprint;
        InvisibiltyCamo.Blueprint = CamoBlueprint;

        _graph = new GridGraph();
        _graphGenerator = new GraphGenerator(_graph);

        Debug.Log("GENERATING GRAPH");
        _graphGenerator.Generate();
        Graph = _graph;
        KeyVertexMapping = _graphGenerator.KeyMapping;
        LockVertexMapping = _graphGenerator.LockMapping;

        _graphDrawer = new GraphGridDrawer(_graph);
        Debug.Log("DRAWING THE GRAPH");
        GraphDrawing = _graphDrawer.Draw(_graphGenerator.GetStartVertex(), _graphGenerator.GetEndVertex());

        _mapBuilder = new MapBuilder(GraphDrawing, SuperWidth, SuperHeight);
        Debug.Log("CREATING SUPERTILES");
        var superTileGrids = _mapBuilder.SuperTileGrid();

        Debug.Log("CREATING TILES");
        var tileGrids = _mapBuilder.TileGrid(superTileGrids, out IEnumerable<EnemyParams> enemies);

        Debug.Log("Creating SUBTILES");
        var subTileGrids = _mapBuilder.SubTileGrid(tileGrids);

        ASubTile.Register<WallSubTile>((ASubTile st) => Instantiate(WallBlueprint));
        ASubTile.Register<FloorSubTile>((ASubTile st) =>
        {

            //return new GameObject();
            return Instantiate(FloorBlueprint);
        });
        ASubTile.Register<DoorSubTile>((ASubTile st) =>
        {
            var door = st as DoorSubTile;
            GameObject doorTileObject = Instantiate(DoorBlueprint);
            var doorObject = doorTileObject.GetComponentInChildren<Door>();
            doorObject.Lock = door.DoorLock;
            doorObject.Lock?.Instances.Add(doorObject);
            doorObject.transform.forward = door.Orientation.ToVector3();
            doorObject.ChangeColor = true;
            doorObject.name = door.Name;
            return doorTileObject;
        });

        ASubTile.Register<WallOfLightSubTile>((ASubTile st) =>
        {
            var wallOfLight = st as WallOfLightSubTile;
            GameObject wallOfLightTileObject = Instantiate(WallOfLightBlueprint);
            var wallOfLightObject = wallOfLightTileObject.GetComponentInChildren<WallOfLight>();
            wallOfLightObject.Lock = wallOfLight.Lock;
            wallOfLightObject.Lock?.Instances.Add(wallOfLightObject);
            wallOfLightObject.transform.forward = wallOfLight.Orientation.ToVector3();
            return wallOfLightTileObject;
        });

        ASubTile.Register<HiddenDoorSubTile>((ASubTile st) =>
        {
            var door = st as HiddenDoorSubTile;
            GameObject doorTileObject = Instantiate(HiddenDoorBlueprint);
            var doorObject = doorTileObject.GetComponentInChildren<Door>();
            doorObject.transform.forward = door.Orientation.ToVector3();
            return doorTileObject;
        });

        ASubTile.Register<RefugeSubTile>((ASubTile st) =>
        {
            var halfRefuge = st as RefugeSubTile;
            GameObject obj = Instantiate(RefugeBlueprint);
            obj.transform.forward = halfRefuge.Orientation.ToVector3();
            return obj;
        });

        ASubTile.Register<NoneSubTile>((ASubTile st) => { return new GameObject(); });

        GameObject level = new("Level");
        GameObject geometry = new("Geometry");
        _geometry = geometry;
        Vector3 offset = new(0, 0, 0);
        geometry.transform.parent = level.transform;

        // Roof
        int w = subTileGrids[subTileGrids.Count - 1].GetLength(0);
        int h = subTileGrids[subTileGrids.Count - 1].GetLength(1);
        subTileGrids.Add(new ASubTile[w, h]);
        _floors = new GameObject[subTileGrids.Count];
        _floorMatrices = new List<Matrix4x4>[subTileGrids.Count];
        for (int i = 0; i < _floorMatrices.Length; i++)
            _floorMatrices[i] = new List<Matrix4x4>();

        _wallMatrices = new List<Matrix4x4>[subTileGrids.Count];
        for (int i = 0; i < _wallMatrices.Length; i++)
            _wallMatrices[i] = new List<Matrix4x4>();

        Debug.Log("SPAWNING OBJECTS");
        for (int floor = subTileGrids.Count - 1; floor >= 0; floor--)
        {
            var grid = subTileGrids[floor];
            var floorGameObject = new GameObject($"Floor {floor}");
            floorGameObject.transform.parent = geometry.transform;
            _floors[floor] = floorGameObject;

            for (int col = 0; col < grid.GetLength(0); col++)
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    if (grid[col, row] == null)
                    {
                        if (floor > 0 && subTileGrids[floor - 1][col, row] != null)
                        {
                            grid[col, row] = new FloorSubTile();
                        } else
                            continue;
                    }

                    //grid[col, row] ??= new FloorSubTile();

                    GameObject obj = grid[col, row].SpawnObject(col, row, floor * _floorHeight);
                    obj.transform.position = obj.transform.position.Added(y: 0.1f * floor);

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

        Debug.Log("Spawning Enemies");
        float scale = 1f;
        var localScale = geometry.transform.localScale;
        //geometry.transform.localScale *= scale;
        geometry.transform.localScale = localScale.Multiplied(x: scale, y: 1f, z: scale);
        foreach (EnemyParams enemy in enemies)
        {
            (int spawnX, int spawnZ) = enemy.Spawn;
            Vector3 spawn = new(spawnX, (enemy.Floor + 0.1f) * _floorHeight, spawnZ);
            spawn = spawn * scale + offset;

            var enemyInstance = Instantiate(
                EnemyBlueprint,
                spawn,
                Quaternion.identity,
                level.transform);

            enemyInstance.transform.parent = _floors[enemy.Floor].transform;


            if (enemy.Behaviour == Behaviour.Patroling)
            {
                List<Vector3> patrol = new();
                foreach ((int x, int z) in enemy.Patrol)
                {
                    patrol.Add(new Vector3(x, (enemy.Floor + 0.1f) * _floorHeight, z) * scale + offset);
                }

                enemyInstance.Patrol(patrol.ToArray(), enemy.PatrolIndex, enemy.Retrace);
            }

            if (enemy.Behaviour == Behaviour.Guarding)
            {
                Debug.Log("SPAWNING GUARD ===========================================================");
                enemyInstance.Guard(spawn, Vector3.right);
                enemyInstance.LookAt(spawn);
            }

            if (enemy.Behaviour == Behaviour.Sleeping)
                enemyInstance.Sleep(spawn);
        }

        // Just offset it for now
        level.transform.position = offset;

        var pos = _mapBuilder.GetEndPosition() * scale + offset;
        int victoryFloor = (int)pos.y;
        pos.y *= _floorHeight;
        var victory = GameObject.Instantiate(VictoryTrigger);
        victory.transform.position = pos;
        victory.transform.parent = _floors[victoryFloor].transform;
        //FindObjectOfType<LevelCamera>().SetPosition(SubTileGrid.GetLength(0), SubTileGrid.GetLength(1), scale, offset);


        var playerSpawn = _mapBuilder.GetSpawnPosition() * scale + offset;
        playerSpawn.y *= _floorHeight;
        Debug.Log($"REPOSITIONING THE PLAYER {playerSpawn}");
        // Spawn player at the correct position
        // Needs to be 1 frame delayed because of bug, when setting position works only occasionally
        StartCoroutine(DelayedSpawn(playerSpawn));

        Map.Drawing = GraphDrawing;
        Map.CreateMap();
        _done = true;
        level.transform.parent = transform;

        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private IEnumerator DelayedSpawn(Vector3 spawnPosition)
    {
        yield return null;
        Player.transform.position = spawnPosition;
    }

    void Update()
    {
        // Render Floors
        RenderParams floorParams = new RenderParams(FloorMaterial);
        floorParams.receiveShadows = true;
        floorParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        RenderParams wallParams = new RenderParams(WallMaterial);
        wallParams.receiveShadows = true;
        wallParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;


        if (_activeFloor.HasValue)
        {
            RenderInstanced(floorParams, FloorMesh, _floorMatrices[_activeFloor.Value]);
            RenderInstanced(wallParams, WallMesh, _wallMatrices[_activeFloor.Value]);
        } else
        {
            foreach (var floor in _floorMatrices)
                RenderInstanced(floorParams, FloorMesh, floor);

            foreach (var wall in _wallMatrices)
                RenderInstanced(wallParams, WallMesh, wall);
        }

        /*/
        RenderParams ps = new RenderParams(FloorMaterial);
        var f = _fuck.Take(1023).ToArray();
        List<Matrix4x4> buffer = new();

        int i = 0;
        while (i < _fuck.Length)
        {
            buffer.Clear();
            for (int j = 0; j < 1023 && i + j < _fuck.Length; j++)
            {
                buffer.Add(_fuck[i + j]);
            }

            i += 1023;
            Graphics.RenderMeshInstanced(ps, FloorMesh, 0, buffer.ToArray());
        }
        //*/
    }

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

    public (int x, int y, int floor) GridCoordinates(Vector3 position)
    {
        Vector3 relative = position - _geometry.transform.position;
        int x = (int)Mathf.Floor(relative.x / (SuperWidth * ATile.WIDTH));
        int y = (int)Mathf.Floor(relative.z / (SuperHeight * ATile.HEIGHT));
        int floor = (int)Mathf.Floor(relative.y / _floorHeight);

        return (x, y, floor);
    }

    public void HighlightFloor(int floor)
    {
        _activeFloor = floor;
        for (int i = 0; i < _floors.Length; i++)
        {
            foreach (var m in _floors[i].GetComponentsInChildren<MeshRenderer>())
                m.enabled = i == floor;
        }
    }

    public void UnHilightFloors()
    {
        for (int i = 0; i < _floors.Length; i++)
        {
            foreach (var m in _floors[i].GetComponentsInChildren<MeshRenderer>())
                m.enabled = true;
        }
        _activeFloor = null;
    }
}