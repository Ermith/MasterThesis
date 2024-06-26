﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Wall of Light to be spawned at exits. The door itself spans two tiles.
/// </summary>
public class WallOfLightSubTile : ASubTile
{
    public WallOfLightLock Lock = null;
    public Directions Orientation = Directions.North;

    protected override GameObject SpawnObject()
    {
        GameObject wallOfLightTileObject = BlueprintManager.Spawn<WallOfLightSubTile>();
        var wallOfLightObject = wallOfLightTileObject.GetComponentInChildren<WallOfLight>();
        wallOfLightObject.Lock = Lock;
        wallOfLightObject.Lock?.Instances.Add(wallOfLightObject);
        wallOfLightObject.transform.forward = Orientation.ToVector3();
        return wallOfLightTileObject;
    }
}

