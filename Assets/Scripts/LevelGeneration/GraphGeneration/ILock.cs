using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.VFX;
using URandom = UnityEngine.Random;

public interface ILock : IRoomFeature
{
    public IKey GetNewKey();
    public IList<ILockObject> Instances { get; }
}

/// <summary>
/// Locks doors in a supertile based in directions given. <see cref="DoorKey"/> unlocks them.
/// </summary>
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
                if (superTile.Get(t.x, t.y) is DoorTile doorTile)
                {
                    doorTile.Lock = this;
                    doorTile.Type = DoorType.Door;
                }

        if (_upExit && superTile.UpExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.UpExit.Value.Item1, superTile.UpExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.Door;
        }

        if (_downExit && superTile.DownExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.DownExit.Value.Item1, superTile.DownExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.Door;
        }
    }

    public DoorLock(Directions exits = Directions.None, bool up = false, bool down = false)
    {
        _exits = exits;
        _upExit = up;
        _downExit = down;
    }
}

/// <summary>
/// Spawns walls of light at supertile exits based on directions given. <see cref="PowerSourceKey"/> unlocks them.
/// </summary>
public class WallOfLightLock : ILock
{
    private Directions _exits;
    private bool _upExit;
    private bool _downExit;
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
                if (superTile.Get(t.x, t.y) is DoorTile doorTile)
                {
                    doorTile.Lock = this;
                    doorTile.Type = DoorType.WallOfLight;
                }


        if (_upExit && superTile.UpExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.UpExit.Value.Item1, superTile.UpExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.WallOfLight;
        }

        if (_downExit && superTile.DownExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.DownExit.Value.Item1, superTile.DownExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.WallOfLight;
        }
    }

    public WallOfLightLock(Directions exits = Directions.None, bool upExit = false, bool downExit = false)
    {
        _exits = exits;
        _upExit = upExit;
        _downExit = downExit;
    }
}

/// <summary>
/// Spawns a patrolling enemy to a supertile. Key to this is <see cref="InvisibiltyCamoKey"/>.
/// </summary>
public class EnemyLock : ILock
{
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        return new InvisibiltyCamoKey();
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
            Behaviour = Behaviour.Patroling,
            Floor = superTile.Floor
        });
    }
}

/// <summary>
/// Spawns security cameras at the exits of a supertile. <see cref="PowerSourceKey"/> disables them.
/// </summary>
public class SecurityCameraLock : ILock
{
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
                var obj = BlueprintManager.Spawn<SecurityCameraLock>();
                var sc = obj.GetComponent<SecurityCameraController>();
                sc.SetOrientation(dir.Opposite());
                sc.Lock = this;
                Instances.Add(sc);
                return obj;
            });
        }
    }
}

/// <summary>
/// Spawns Death Traps on free tiles of a given supertile.
/// <see cref="TrapDisarmingKitKey"/> can be used to disable them instantly instead of a long interaction.
/// </summary>
public class DeathTrapLock : ILock
{
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        return new TrapDisarmingKitKey();
    }

    public void Implement(SuperTileDescription superTile)
    {
        var freeTiles = superTile.FreeTiles.ToArray();
        if (freeTiles.Length <= 0) return;

        for (int i = 0; i < freeTiles.Length; i++)
        {
            int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
            (int x, int y) = freeTiles[i];
            var tile = superTile.Get(x, y);
            //superTile.FreeTiles.Remove((x, y));
            tile.Objects.Add(() =>
            {
                var obj = BlueprintManager.Spawn<DeathTrapLock>();
                var lo = obj.GetComponentsInChildren<ILockObject>();

                foreach (var l in lo)
                {
                    l.Lock = this;
                    Instances.Add(l);
                }
                return obj;
            });
        }
    }
}

/// <summary>
/// Spawns Sound Traps on free tiles of a given supertile.
/// <see cref="TrapDisarmingKitKey"/> can be used to disable them instantly instead of a long interaction.
/// </summary>
public class SoundTrapLock : ILock
{
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        return new TrapDisarmingKitKey();
    }

    public void Implement(SuperTileDescription superTile)
    {
        var freeTiles = superTile.FreeTiles.ToArray();
        if (freeTiles.Length <= 0) return;

        for (int i = 0; i < freeTiles.Length; i++)
        {
            int tileIndex = URandom.Range(0, superTile.FreeTiles.Count - 1);
            (int x, int y) = freeTiles[i];
            var tile = superTile.Get(x, y);
            //superTile.FreeTiles.Remove((x, y));
            tile.Objects.Add(() =>
            {
                var obj = BlueprintManager.Spawn<SoundTrapLock>();
                var lo = obj.GetComponentsInChildren<ILockObject>();

                foreach (var l in lo)
                {
                    l.Lock = this;
                    Instances.Add(l);
                }
                return obj;
            });
        }
    }
}

/// <summary>
/// Spawns doors that look like walls in a supertile based on exits given. Does not have a key.
/// </summary>
public class HiddenDoorLock : ILock
{
    private Directions _exits;
    private bool _up;
    private bool _down;
    public IList<ILockObject> Instances { get; } = new List<ILockObject>();

    public IKey GetNewKey()
    {
        return null;
    }

    public void Implement(SuperTileDescription superTile)
    {
        foreach (Directions dir in _exits.Enumerate())
            if (superTile.ExitsTiles.TryGetValue(dir, out (int x, int y) t))
                if (superTile.Get(t.x, t.y) is DoorTile doorTile)
                {
                    doorTile.Lock = this;
                    doorTile.Type = DoorType.HiddenDoor;
                }

        if (_up && superTile.UpExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.UpExit.Value.Item1, superTile.UpExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.HiddenDoor;
        }

        if (_down && superTile.DownExit.HasValue)
        {
            var doorTile = superTile.Get(superTile.DownExit.Value.Item1, superTile.DownExit.Value.Item2) as DoorTile;
            doorTile.Lock = this;
            doorTile.Type = DoorType.HiddenDoor;
        }
    }

    public HiddenDoorLock(Directions exits = Directions.None, bool up = false, bool down = false)
    {
        _exits = exits;
        _up = up;
        _down = down;
    }
}
