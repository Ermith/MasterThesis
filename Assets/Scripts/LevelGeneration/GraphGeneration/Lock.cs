using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

using URandom = UnityEngine.Random;

public interface IRoomFeature
{
    public void Implement(SuperTileDescription superTile);
}

public interface Lock
{
    public Key GetNewKey();
    public void Implement(SuperTileDescription superTile);
}

public interface Key
{
    Lock Lock { get; }
    public void Implement(SuperTileDescription superTile);
}

public class DoorLock : Lock
{
    public Key GetNewKey() => new DoorKey(this);

    public void Implement(SuperTileDescription superTile)
    {
        foreach (Directions dir in superTile.Exits.Enumerate())
            if (superTile.ExitsTiles.TryGetValue(dir, out (int x, int y) t))
                if (superTile.Get(t.x, t.y) is DoorTile door)
                    door.Lock = this;
    }
}

public class DoorKey : Key
{
    public static GameObject KeyBlueprint;
    public DoorKey(Lock l)
    {
        _lock = l;
    }

    private readonly Lock _lock;
    public Lock Lock => _lock;

    public void Implement(SuperTileDescription superTile)
    {
        int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
        (int x, int y) = superTile.FreeTiles.ToArray()[tileIndex];
        var tile = superTile.Get(x, y);
        superTile.FreeTiles.Remove((x, y));
        tile.Objects.Add(KeyBlueprint);
    }
}

public class EnemyLock : Lock
{
    public Key GetNewKey()
    {
        return null;
    }

    public void Implement(SuperTileDescription superTile)
    {
        List<(int, int)> patrol = new();
        foreach ((int x, int y) in superTile.PatrolPath)
            patrol.Add((x, y));

        int spawnIndex = URandom.Range(0, patrol.Count - 1);
        superTile.Enemies.Add(new EnemyParams
        {
            Patrol = patrol,
            Spawn = 0
        });
    }
}