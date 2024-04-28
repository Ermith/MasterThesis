using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Gives ability to respond to triggers when the player collides with this object.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SmartCollider : MonoBehaviour
{
    internal delegate void TriggerResponse(PlayerController player);
    /// <summary>
    /// The player enters this object.
    /// </summary>
    internal event TriggerResponse _triggerResponse;
    /// <summary>
    /// The player leaves this object.
    /// </summary>
    internal event TriggerResponse _triggerLeaveResponse;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        _triggerResponse?.Invoke(other.GetComponent<PlayerController>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
            return;

        _triggerLeaveResponse?.Invoke(other.GetComponent<PlayerController>());
    }
}
