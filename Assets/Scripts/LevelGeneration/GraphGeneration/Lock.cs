using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface Lock
{
    public Key GetNewKey();
    public void Implement(ATile[,] tileGrid, int x, int y, int width, int height);
}

public interface Key
{
    Lock Lock { get; }
    public void Implement(ATile[,] tileGrid, int x, int y, int width, int height);
}

public class DoorLock : Lock
{
    public Key GetNewKey() => new DoorKey(this);

    public void Implement(ATile[,] tileGrid, int x, int y, int width, int height)
    {
        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
                if (tileGrid[i, j] is DoorTile door)
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

    public void Implement(ATile[,] tileGrid, int x, int y, int width, int height)
    {
        tileGrid[x + width / 2, y + height / 2].Objects.Add(KeyBlueprint);
    }
}