using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

static class BlueprintManager
{
    private static Dictionary<Type, Func<GameObject>> _spawner = new();

    public static void Reset() => _spawner.Clear();

    public static void Register<T>(Func<GameObject> spawnFunction) =>
        _spawner[typeof(T)] = spawnFunction;

    public static GameObject Spawn<T>() => _spawner[typeof(T)]();
    public static GameObject Spawn(Type type) => _spawner[type]();
}