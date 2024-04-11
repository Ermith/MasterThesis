using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    GridGraph _graph;
    GraphGenerator _graphGenerator;
    GraphGridDrawer _graphDrawer;
    MapBuilder _mapBuilder;

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
        ASubTile.Register<FloorSubTile>((ASubTile st) => Instantiate(FloorBlueprint));
        ASubTile.Register<DoorSubTile>((ASubTile st) =>
        {
            var door = st as DoorSubTile;
            GameObject doorTileObject = Instantiate(DoorBlueprint);
            var doorObject = doorTileObject.GetComponentInChildren<Door>();
            doorObject.Lock = door.DoorLock;
            doorObject.Lock?.Instances.Add(doorObject);
            doorObject.transform.forward = door.Orientation.ToVector3();
            doorObject.ChangeColor = true;
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
        Vector3 offset = new(0, 0, 0);
        geometry.transform.parent = level.transform;
        int floorHeight = 3;

        Debug.Log("SPAWNING OBJECTS");
        for (int floor = 0; floor < subTileGrids.Count; floor++)
        {
            var grid = subTileGrids[floor];
            for (int col = 0; col < grid.GetLength(0); col++)
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    grid[col, row] ??= new FloorSubTile();
                    GameObject obj = grid[col, row].SpawnObject(col, row, floor * floorHeight);
                    obj.transform.parent = geometry.transform;
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
            Vector3 spawn = new(spawnX, 0, spawnZ);
            spawn = spawn * scale + offset;

            var enemyInstance = Instantiate(
                EnemyBlueprint,
                spawn,
                Quaternion.identity,
                level.transform);


            if (enemy.Behaviour == Behaviour.Patroling)
            {
                List<Vector3> patrol = new();
                foreach ((int x, int z) in enemy.Patrol)
                {
                    patrol.Add(new Vector3(x, 0, z) * scale + offset);
                }

                enemyInstance.Patrol(patrol.ToArray(), enemy.PatrolIndex, enemy.Retrace);
            }

            if (enemy.Behaviour == Behaviour.Guarding)
            {
                Debug.Log("SPAWNING GUARD ===========================================================");
                enemyInstance.Guard(spawn, Vector3.right);
            }

            if (enemy.Behaviour == Behaviour.Sleeping)
                enemyInstance.Sleep(spawn);
        }

        // Just offset it for now
        level.transform.position = offset;
        
        var pos = _mapBuilder.GetEndPosition() * scale + offset;
        pos.y *= floorHeight;
        GameObject.Instantiate(VictoryTrigger).transform.position = pos;
        //FindObjectOfType<LevelCamera>().SetPosition(SubTileGrid.GetLength(0), SubTileGrid.GetLength(1), scale, offset);


        var playerSpawn = _mapBuilder.GetSpawnPosition() * scale + offset;
        playerSpawn.y *= floorHeight;
        Debug.Log($"REPOSITIONING THE PLAYER {playerSpawn}");
        // Spawn player at the correct position
        // Needs to be 1 frame delayed because of bug, when setting position works only occasionally
        StartCoroutine(DelayedSpawn(playerSpawn));

        Map.Drawing = GraphDrawing;
        Map.CreateMap();
    }

    private IEnumerator DelayedSpawn(Vector3 spawnPosition)
    {
        yield return null;
        Player.transform.position = spawnPosition;
    }
}