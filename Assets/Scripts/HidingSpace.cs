using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingSpace : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            other.GetComponent<PlayerController>().Refuge = true;
    }
}
