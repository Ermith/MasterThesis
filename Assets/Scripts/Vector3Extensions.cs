using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static void SetX(ref this Vector3 vec, float x) => vec.x = x;
    public static void SetY(ref this Vector3 vec, float y) => vec.y = y;
    public static void SetZ(ref this Vector3 vec, float z) => vec.z = z;

    public static void AddX(ref this Vector3 vec, float x) => vec.x += x;
    public static void AddY(ref this Vector3 vec, float y) => vec.y += y;
    public static void AddZ(ref this Vector3 vec, float z) => vec.z += z;

}