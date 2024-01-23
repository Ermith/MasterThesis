using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingSpace : SmartCollider
{
    private void Start()
    {
        _triggerResponse += (Player player) =>
        {
            player.SetHidden(true);
            Debug.Log("Player in Hiding");
        };

        _triggerLeaveResponse += (Player player) =>
        {
            player.SetHidden(false);
            Debug.Log("NO PLAYER HIDE");
        };
    }
}
