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
    private static Dictionary<Type, GameObject> _objects = new();
    public static void Register<T>(GameObject obj) where T : ASubTile
    {
        _objects[typeof(T)] = obj;
    }

    public virtual GameObject SpawnObject(int x, int y)
    {
        var obj = GameObject.Instantiate(_objects[this.GetType()]);
        obj.transform.position = new Vector3(x, 0, y);
        obj.name = GetType().Name;
        return obj;
    }
}