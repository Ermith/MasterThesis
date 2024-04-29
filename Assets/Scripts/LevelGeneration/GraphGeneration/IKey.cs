using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using URandom = UnityEngine.Random;

public interface IKey : IRoomFeature
{
    IList<ILock> Locks { get; }
    bool Guarded { get; set; }
}

/// <summary>
/// A power box used for <see cref="WallOfLightLock"/> and <see cref="SecurityCameraLock"/>.
/// </summary>
public class PowerSourceKey : IKey
{
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
            var gameObject = BlueprintManager.Spawn<PowerSourceKey>();
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

/// <summary>
/// A key to open <see cref="DoorLock"/>.
/// </summary>
public class DoorKey : IKey
{
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
            var obj = BlueprintManager.Spawn<DoorKey>();
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

/// <summary>
/// Does not complement any lock. Simply spawns a side objective.
/// </summary>
public class SideObjectiveKey : IKey
{
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
            var obj = BlueprintManager.Spawn<SideObjectiveKey>();
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

/// <summary>
/// Used for disarming <see cref="DeathTrapLock"/> and <see cref="SoundTrapLock"/>.
/// </summary>
public class TrapDisarmingKitKey : IKey
{
    public IList<ILock> Locks { get; } = new List<ILock>();
    public bool Guarded { get; set; } = false;

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
            var obj = BlueprintManager.Spawn<TrapDisarmingKitKey>();
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

/// <summary>
/// Is key to <see cref="EnemyLock"/>. Makes the player invisible to the <see cref="Sight"/> component.
/// </summary>
public class InvisibiltyCamoKey : IKey
{
    public IList<ILock> Locks { get; } = new List<ILock>();
    public bool Guarded { get; set; } = false;

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
            var obj = BlueprintManager.Spawn<InvisibiltyCamoKey>();
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
