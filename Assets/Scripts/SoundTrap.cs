using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrap : SmartCollider {

    public float SoundRange = 15f;

    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            Debug.Log("Sound Trap Triggered");
            GameController.AudioManager.AudibleEffect(gameObject, player.transform.position, SoundRange);
            GameController.AudioManager.Play("GlassShatter");
        };
    }

}
