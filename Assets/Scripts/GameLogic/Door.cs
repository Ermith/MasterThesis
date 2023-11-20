using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorLock DoorLock = null;

    [SerializeField]
    private GameObject _doorObject;

    void Start()
    {
        _doorObject.GetComponent<Collider>().enabled = DoorLock == null;
        _doorObject.GetComponent<MeshRenderer>().material.color = 
            (DoorLock == null) ? Color.green : Color.red;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
