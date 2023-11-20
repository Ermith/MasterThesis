using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ATile
{
    public const int WIDTH = 3, HEIGHT = 3;
    public List<GameObject> Objects = new();
    public abstract void BuildSubTiles(int x, int y, ASubTile[,] subTileGrid);
}
