using UnityEngine;

/// <summary>
/// Spawns a tile that player can take refuge on.
/// </summary>
public class RefugeSubTile : ASubTile
{
    public Directions Orientation;

    public RefugeSubTile(Directions orientation = Directions.North)
    {
        Orientation = orientation;
    }

    protected override GameObject SpawnObject()
    {
        GameObject obj = BlueprintManager.Spawn<RefugeSubTile>();
        obj.transform.forward = Orientation.ToVector3();
        return obj;
    }
}