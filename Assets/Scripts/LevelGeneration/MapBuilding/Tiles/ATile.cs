using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

/// <summary>
/// Abstract class for all the Tiles.
/// </summary>
public abstract class ATile
{
    public const int WIDTH = 6, HEIGHT = 6;
    public int HalfWidth => (WIDTH) / 2;
    public int HalfHeight => (HEIGHT) / 2;

    public List<Func<GameObject>> Objects = new();
    public EnemyParams Guard = null;
    public abstract void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid);

    /// <summary>
    /// Transform coordinates inside supertiles to subtilegrid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static (int, int) FromSuper(int x, int y) => (
        x * WIDTH,
        y * HEIGHT
    );

    /// <summary>
    /// Transform coordinates inside supertiles to subtilegrid in the middle of this Tile.
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static (int, int) FromSuperMid(int x, int y) => (
        x * WIDTH + WIDTH / 2,
        y * HEIGHT + HEIGHT / 2
    );
}
