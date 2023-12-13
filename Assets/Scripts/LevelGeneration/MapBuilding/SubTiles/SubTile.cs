using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public abstract class ASubTile
{
    private static Dictionary<Type, Func<ASubTile, GameObject>> _spawnFunctions = new();

    public List<Func<GameObject>> Objects = new();
    public static void Register<T>(Func<ASubTile, GameObject> spawnFunction) where T : ASubTile
    {
        _spawnFunctions[typeof(T)] = spawnFunction;
    }

    public virtual GameObject SpawnObject(int x, int y)
    {
        var tileObj = _spawnFunctions[GetType()](this);
        Vector3 pos = new(x, 0, y);
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