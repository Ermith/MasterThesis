using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Abstract class for all subitles.
/// </summary>
public abstract class ASubTile
{
    public List<Func<GameObject>> Objects = new();

    /// <summary>
    /// Instantiates the subtile object.
    /// </summary>
    /// <returns></returns>
    protected abstract GameObject SpawnObject();

    /// <summary>
    /// Spawns the subtile object and all of the objects attatched to it. Like keys for example.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public virtual GameObject Spawn(int x, int y, int z)
    {
        var tileObj = SpawnObject();
        Vector3 pos = new(x, z, y);
        tileObj.transform.position = pos;
        tileObj.name = GetType().Name;

        foreach (var obj in Objects)
        {
            var keyObj = obj();
            keyObj.transform.parent = tileObj.transform;
            keyObj.transform.localPosition = Vector3.zero;
        }

        return tileObj;
    }
}