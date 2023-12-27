using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class SmartCollider : MonoBehaviour
{
    internal delegate void TriggerResponse(PlayerController player);
    internal event TriggerResponse _triggerResponse;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        _triggerResponse.Invoke(other.GetComponent<PlayerController>());
    }
}
