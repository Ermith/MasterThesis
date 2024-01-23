using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingKey : SmartCollider, IKeyObject
{
    public IKey MyKey { get; set; }

    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            player.AddKey(MyKey);
            GameController.AudioManager.Play("Beep", position: transform.position);
            Destroy(gameObject);
        };
    }
}
