using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour, IInteractableObject, IKeyObject
{
    public IKey MyKey { get; set; }

    public bool CanInteract => enabled;

    public void Interact(Player player)
    {
        if (!CanInteract) return;

        foreach (var @lock in MyKey.Locks)
            foreach (var lockObject in @lock.Instances)
            {
                Debug.Log("UNLOCKING");
                lockObject.Unlock();
            }

        var audio = GameController.AudioManager.Play("Cut");
        GameController.ExecuteAfter(() => GameController.AudioManager.Play("ElectricDischarge"), audio.clip.length);

        GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        enabled = false;
    }

    public string InteractionPrompt() => "Cut Power";
}
