using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Hidden door to be spawned at exits. The door itself spans two tiles.
/// </summary>
class HiddenDoorSubTile : ASubTile
{
    public Directions Orientation = Directions.North;

    protected override GameObject SpawnObject()
    {
        var doorTileObject = BlueprintManager.Spawn<HiddenDoorSubTile>();
        var doorObject = doorTileObject.GetComponentInChildren<Door>();
        doorObject.transform.forward = Orientation.ToVector3();
        return doorTileObject;
    }
}