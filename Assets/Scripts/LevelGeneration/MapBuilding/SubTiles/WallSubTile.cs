using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Impassable wall.
/// </summary>
public class WallSubTile : ASubTile
{
    protected override GameObject SpawnObject()
    {
        return BlueprintManager.Spawn<WallSubTile>();
    }
}