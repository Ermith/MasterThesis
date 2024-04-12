using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.VFX;
using URandom = UnityEngine.Random;

public interface IRoomFeature
{
    public void Implement(SuperTileDescription superTile);
}

public interface ILock
{
    public IKey GetNewKey();
    public void Implement(SuperTileDescription superTile);
    public IList<ILockObject> Instances { get; }
}

public interface IKey
{
    IList<ILock> Locks { get; }
    bool Guarded { get; set; }
    public void Implement(SuperTileDescription superTile);
}

public class DoorLock : ILock
{
    private Directions _exits;
    private bool _upExit;
    private bool _downExit;
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        var doorKey = new DoorKey();
        doorKey.Locks.Add(this);
        return doorKey;
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach (Directions dir in _exits.Enumerate())
            if (superTile.ExitsTiles.TryGetValue(dir, out (int x, int y) t))
                if (superTile.Get(t.x, t.y) is DoorTile door)
                    door.Lock = this;

        if (_upExit && superTile.UpExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.UpExit.Value.Item1, superTile.UpExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
        }

        if (_downExit && superTile.DownExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.DownExit.Value.Item1, superTile.DownExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
        }
    }

    public DoorLock(Directions exits = Directions.None, bool up = false, bool down = false)
    {
        _exits = exits;
        _upExit = up;
        _downExit = down;
    }
}

public class WallOfLightLock : ILock
{
    private Directions _exits;
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        var doorKey = new PowerSourceKey();
        doorKey.Locks.Add(this);
        return doorKey;
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach (Directions dir in _exits.Enumerate())
            if (superTile.ExitsTiles.TryGetValue(dir, out (int x, int y) t))
                if (superTile.Get(t.x, t.y) is DoorTile door)
                {
                    door.Lock = this;
                    door.Type = DoorType.WallOfLight;
                }
    }

    public WallOfLightLock(Directions exits)
    {
        _exits = exits;
    }
}

public class DoorKey : IKey
{
    public static GameObject Blueprint;

    public IList<ILock> Locks { get; } = new List<ILock>();
    public bool Guarded { get; set; } = true;

    public void Implement(SuperTileDescription superTile)
    {
        if (superTile.FreeTiles.Count == 0)
            return;

        int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
        (int x, int y) = superTile.FreeTiles.ToArray()[tileIndex];
        var tile = superTile.Get(x, y);
        superTile.FreeTiles.Remove((x, y));
        tile.Objects.Add(() =>
        {
            var obj = GameObject.Instantiate(Blueprint);
            obj.GetComponent<IKeyObject>().MyKey = this;
            return obj;
        });

        if (Guarded)
        {
            (int spawnX, int spawnY) = ATile.FromSuperMid(superTile.X + x, superTile.Y + y);
            tile.Guard = new EnemyParams
            {
                Behaviour = Behaviour.Guarding,
                Spawn = (spawnX - 1, spawnY),
                Floor = superTile.Floor
            };

            superTile.Enemies.Add(tile.Guard);
        }
    }
}

public class EnemyLock : ILock
{
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
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
            Spawn = patrol[spawnIndex],
            PatrolIndex = spawnIndex,
            Lock = this,
            Behaviour = Behaviour.Patroling
        });
    }
}

public class SecurityCameraLock : ILock
{
    public static GameObject Blueprint;

    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        PowerSourceKey key = new();
        key.Locks.Add(this);
        return key;
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach ((Directions dir, (int x, int y)) in superTile.ExitsTiles)
        {
            var exit = superTile.Get(x, y);
            exit.Objects.Add(() =>
            {
                var obj = GameObject.Instantiate(Blueprint);
                var sc = obj.GetComponent<SecurityCameraController>();
                sc.SetOrientation(dir.Opposite());
                sc.Lock = this;
                Instances.Add(sc);
                return obj;
            });
        }
    }
}

public class PowerSourceKey : IKey
{
    public static GameObject Blueprint;

    public bool Guarded { get; set; } = true;

    public IList<ILock> Locks { get; } = new List<ILock>();

    public void Implement(SuperTileDescription superTile)
    {
        if (superTile.FreeTiles.Count == 0) return;

        int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
        (int x, int y) = superTile.FreeTiles.ToArray()[tileIndex];
        var tile = superTile.Get(x, y);
        superTile.FreeTiles.Remove((x, y));

        tile.Objects.Add(() =>
        {
            var gameObject = GameObject.Instantiate(Blueprint);
            gameObject.GetComponent<IKeyObject>().MyKey = this;
            return gameObject;
        });

        if (Guarded)
        {
            (int spawnX, int spawnY) = ATile.FromSuperMid(superTile.X + x, superTile.Y + y);
            tile.Guard = new EnemyParams
            {
                Behaviour = Behaviour.Guarding,
                Spawn = (spawnX - 1, spawnY),
                Floor = superTile.Floor
            };

            superTile.Enemies.Add(tile.Guard);
        }
    }
}

public class TrapLock : ILock
{
    public static GameObject Blueprint;

    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
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
            tile.Objects.Add(() =>
            {
                var obj = GameObject.Instantiate(Blueprint);
                var lo = obj.GetComponent<ILockObject>();
                lo.Lock = this;
                Instances.Add(lo);
                return obj;
            });
        }
    }
}

public class SoundTrapLock : ILock
{
    public static GameObject Blueprint;

    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
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
            tile.Objects.Add(() =>
            {
                var obj = GameObject.Instantiate(Blueprint);
                var lo = obj.GetComponent<ILockObject>();
                lo.Lock = this;
                Instances.Add(lo);
                return obj;
            });
        }
    }
}

public class HiddenDoorLock : ILock
{
    private Directions _exits;
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        return null;
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach (Directions dir in _exits.Enumerate())
            if (superTile.ExitsTiles.TryGetValue(dir, out (int x, int y) t))
                if (superTile.Get(t.x, t.y) is DoorTile door)
                {
                    door.Lock = this;
                    door.Type = DoorType.HiddenDoor;
                }
    }

    public HiddenDoorLock(Directions exits)
    {
        _exits = exits;
    }
}