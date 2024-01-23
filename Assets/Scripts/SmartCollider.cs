using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class SmartCollider : MonoBehaviour
{
    internal delegate void TriggerResponse(Player player);
    internal event TriggerResponse _triggerResponse;
    internal event TriggerResponse _triggerLeaveResponse;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        _triggerResponse?.Invoke(other.GetComponent<Player>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
            return;

        _triggerLeaveResponse?.Invoke(other.GetComponent<Player>());
    }
}
