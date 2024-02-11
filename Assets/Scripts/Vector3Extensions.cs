using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Set(
        this Vector3 vec,
        float x = float.NaN,
        float y = float.NaN,
        float z = float.NaN
        ) => new Vector3(
                x: float.IsNaN(x) ? vec.x : x,
                y: float.IsNaN(y) ? vec.y : y,
                z: float.IsNaN(z) ? vec.z : z);

    public static Vector3 Added(
        this Vector3 vec,
        float x = 0,
        float y = 0,
        float z = 0)
        => new Vector3(vec.x + x, vec.y + y, vec.z + z);

    public static Vector3 Multiplied(
        this Vector3 vec,
        float x = 1,
        float y = 1,
        float z = 1)
        => new Vector3(vec.x * x, vec.y * y, vec.z * z);
}