using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Useful extensions for <see cref="Vector3"/> for modifying the vector.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Returns the vector but with replaced fields that are specified.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 Set(
        this Vector3 vec,
        float x = float.NaN,
        float y = float.NaN,
        float z = float.NaN
        ) => new Vector3(
                x: float.IsNaN(x) ? vec.x : x,
                y: float.IsNaN(y) ? vec.y : y,
                z: float.IsNaN(z) ? vec.z : z);


    /// <summary>
    /// Returns the vector but with fields added onto that are specified.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 Added(
        this Vector3 vec,
        float x = 0,
        float y = 0,
        float z = 0)
        => new Vector3(vec.x + x, vec.y + y, vec.z + z);


    /// <summary>
    /// Returns the vector but with multiplied fields that are specified.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 Multiplied(
        this Vector3 vec,
        float x = 1,
        float y = 1,
        float z = 1)
        => new Vector3(vec.x * x, vec.y * y, vec.z * z);
}