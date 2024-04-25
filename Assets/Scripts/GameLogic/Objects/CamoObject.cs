using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamoObject : SmartCollider, IKeyObject
{
    public IKey MyKey { get; set; }
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
