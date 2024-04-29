using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Spawns only empty game object.
/// </summary>
class NoneSubTile : ASubTile
{
    protected override GameObject SpawnObject()
    {
        return BlueprintManager.Spawn<NoneSubTile>();
    }
}
