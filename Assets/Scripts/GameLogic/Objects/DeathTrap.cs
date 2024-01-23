using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrap : SmartCollider, ILockObject
{
    public ILock Lock { get; set; }

    public void Unlock()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            Debug.Log("Death Trap Triggered");
            player.Die();
        };
    }
}
