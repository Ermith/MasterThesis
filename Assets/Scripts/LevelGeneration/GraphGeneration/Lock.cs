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
    public static GameObject Blueprint;
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
        tile.Objects.Add(() => GameObject.Instantiate(Blueprint));
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

public class SecurityCameraLock : Lock
{
    public static GameObject Blueprint;
    public Key GetNewKey()
    {
        return new SecurityCameraKey(this);
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach ((Directions dir, (int x, int y)) in superTile.ExitsTiles)
        {
            var exit = superTile.Get(x, y);
            exit.Objects.Add(() =>
            {
                var obj = GameObject.Instantiate(Blueprint);
                obj.GetComponent<SecurityCameraController>().SetOrientation(dir.Opposite());
                return obj;
            });
        }
    }
}

public class SecurityCameraKey : Key
{
    public static GameObject Blueprint;
    public SecurityCameraKey(Lock l)
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
        tile.Objects.Add(() => GameObject.Instantiate(Blueprint));
    }
}

public class TrapLock : Lock
{
    public static GameObject Blueprint;
    public Key GetNewKey()
    {
        return null;
    }

    public void Implement(SuperTileDescription superTile)
    {
        var freeTiles = superTile.FreeTiles.ToArray();
        if (freeTiles.Length <= 0) return;

        for (int i = 0; i < freeTiles.Length / 1; i++)
        {
            int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
            (int x, int y) = freeTiles[tileIndex];
            var tile = superTile.Get(x, y);
            superTile.FreeTiles.Remove((x, y));
            tile.Objects.Add(() => GameObject.Instantiate(Blueprint));
        }
    }
}

public class SoundTrapLock : Lock
{
    public static GameObject Blueprint;
    public Key GetNewKey()
    {
        return null;
    }

    public void Implement(SuperTileDescription superTile)
    {
        var freeTiles = superTile.FreeTiles.ToArray();
        if (freeTiles.Length <= 0) return;


        for (int i = 0; i < freeTiles.Length / 1; i++)
        {
            int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
            (int x, int y) = freeTiles[tileIndex];
            var tile = superTile.Get(x, y);
            superTile.FreeTiles.Remove((x, y));
            tile.Objects.Add(() => GameObject.Instantiate(Blueprint));
        }
    }
}