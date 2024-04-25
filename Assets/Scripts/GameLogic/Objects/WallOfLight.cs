using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallOfLight : MonoBehaviour, ILockObject
{
    public ILock Lock { get; set; }

    public void Unlock()
    {
        transform.Find("Hinge").gameObject.SetActive(false);
    }
}
