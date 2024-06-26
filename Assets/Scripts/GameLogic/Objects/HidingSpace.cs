using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Colliding with this object makes the player hidden.
/// </summary>
public class HidingSpace : SmartCollider
{
    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            player.SetHidden(true);
            Debug.Log("Player in Hiding");
        };

        _triggerLeaveResponse += (PlayerController player) =>
        {
            player.SetHidden(false);
            Debug.Log("NO PLAYER HIDE");
        };
    }
}
