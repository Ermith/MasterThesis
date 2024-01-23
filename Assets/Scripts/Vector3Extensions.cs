using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 SetX(ref this Vector3 vec, float x) => new Vector3(x, vec.y, vec.z);
    public static Vector3 SetY(ref this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
    public static Vector3 SetZ(ref this Vector3 vec, float z) => new Vector3(vec.x, vec.y, z);

    public static Vector3 AddX(ref this Vector3 vec, float x) => new Vector3(vec.x + x, vec.y ,vec.z);
    public static Vector3 AddY(ref this Vector3 vec, float y) => new Vector3(vec.x, vec.y + y, vec.z);
    public static Vector3 AddZ(ref this Vector3 vec, float z) => new Vector3(vec.x, vec.y, vec.z + z);

    public static Vector3 MultiplyX(ref this Vector3 vec, float x) => new Vector3(vec.x * x, vec.y, vec.z);
    public static Vector3 MultiplyY(ref this Vector3 vec, float y) => new Vector3(vec.x, vec.y * y, vec.z);
    public static Vector3 MultiplyZ(ref this Vector3 vec, float z) => new Vector3(vec.x, vec.y, vec.z * z);

}