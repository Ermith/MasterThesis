using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lock Object that blocks the way. Is disabled by a power source.
/// </summary>
public class WallOfLight : MonoBehaviour, ILockObject
{
    public ILock Lock { get; set; }

    public void Unlock()
    {
        transform.Find("Hinge").gameObject.SetActive(false);
    }
}
