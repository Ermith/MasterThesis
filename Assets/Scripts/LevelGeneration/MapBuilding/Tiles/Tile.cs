using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public abstract class ATile
{
    public const int WIDTH = 3, HEIGHT = 3;
    public List<Func<GameObject>> Objects = new();
    public abstract void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid);

    public static (int, int) FromSuper(int x, int y) => (
        x * WIDTH,
        y * HEIGHT
    );

    public static (int, int) FromSuperMid(int x, int y) => (
        x * WIDTH + WIDTH / 2,
        y * HEIGHT + HEIGHT / 2
    );
}
