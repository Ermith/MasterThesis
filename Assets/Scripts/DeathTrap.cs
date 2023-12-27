using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrap : SmartCollider
{
    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            Debug.Log("Death Trap Triggered");
            player.Die();
        };
    }
}
