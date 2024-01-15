using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour, IUsableObject, IKeyObject
{
    public IKey MyKey { get; set; }

    public bool IsUsable => enabled;

    public void Use(PlayerController player)
    {
        if (!IsUsable) return;

        foreach (var @lock in MyKey.Locks)
            foreach (var lockObject in @lock.Instances)
            {
                Debug.Log("UNLOCKING");
                lockObject.Unlock();
            }

        var audio = GameController.AudioManager.Play("Cut");
        GameController.Instance.ExecuteAfter(() => GameController.AudioManager.Play("ElectricDischarge"), audio.clip.length);

        GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        enabled = false;
    }

    public string UsePrompt() => "Cut Power";
}
