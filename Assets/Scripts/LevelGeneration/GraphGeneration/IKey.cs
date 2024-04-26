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

public class SideObjective : IKey
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

public class TrapDisarmingKit : IKey
{
    public static GameObject Blueprint;

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

public class InvisibiltyCamo : IKey
{
    public static GameObject Blueprint;

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
