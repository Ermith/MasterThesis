using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives the player <see cref="TrapDisarmingKitKey"/> when picked up by moving through this object.
/// </summary>
public class TrapKitObject : SmartCollider, IKeyObject
{
    public IKey MyKey { get; set; }
    [Tooltip("Number of Trap Disarming Kits recieved when picked up.")]
    public int Count = 5;

    // Start is called before the first frame update
    void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            player.TrapKitCount += Count;
            GameController.AudioManager.Play("Beep", position: transform.position);
            Destroy(gameObject);
        };
    }
}
