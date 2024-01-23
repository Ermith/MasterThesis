using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrap : SmartCollider, ILockObject
{

    public float SoundRange = 15f;

    public ILock Lock { get; set; }

    public void Unlock()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            Debug.Log("Sound Trap Triggered");
            GameController.AudioManager.AudibleEffect(gameObject, player.transform.position, SoundRange);
            GameController.AudioManager.Play("GlassShatter", position: transform.position);
            Destroy(gameObject);
        };
    }

}
