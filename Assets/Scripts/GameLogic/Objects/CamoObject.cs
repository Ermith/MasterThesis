using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives the player <see cref="InvisibiltyCamoKey"/> when picked up by moving through this object.
/// </summary>
public class CamoObject : SmartCollider, IKeyObject
{
    public IKey MyKey { get; set; }
    [Tooltip("Number of camos the player will recieve.")]
    public int Count = 1;

    // Start is called before the first frame update
    void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            player.CamoCount += Count;
            GameController.AudioManager.Play("Beep", position: transform.position);
            Destroy(gameObject);
        };
    }
}
