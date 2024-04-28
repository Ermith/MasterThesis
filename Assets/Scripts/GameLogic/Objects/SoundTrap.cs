using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes a loud noise when the player moves through this object. Can be disabled by interaction.
/// Interaction time reduced to 0 if the player has <see cref="TrapDisarmingKitKey"/>. Consumes it on use.
/// </summary>
public class SoundTrap : SmartCollider, ILockObject, IInteractableObject
{

    public float SoundRange = 15f;

    public ILock Lock { get; set; }

    public bool CanInteract => true;

    public InteractionType InteractionType => InteractionType.Continuous;

    public float InteractionTime = 1f;
    private float _interactionTimer;
    private bool _lastInteract = false;

    /// <summary>
    /// Disables the trap. Takes time unless the player has <see cref="TrapDisarmingKitKey"/>. The kit is consumed.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public float Interact(PlayerController player)
    {
        if (_interactionTimer >= InteractionTime)
            GameController.AudioManager.Play("Click");

        _interactionTimer -= Time.deltaTime;

        if (player.TrapKitCount > 0)
        {
            _interactionTimer = 0;
            player.TrapKitCount--;
        }

        if (_interactionTimer <= 0)
            Unlock();

        _lastInteract = true;

        return 1 - _interactionTimer / InteractionTime;
    }

    public string InteractionPrompt()
    {
        return "Disarm";
    }

    public void Unlock()
    {
        GameController.AudioManager.Play("Cut");
        gameObject.SetActive(false);
    }

    private void Start()
    {
        _interactionTimer = InteractionTime;
        _triggerResponse += (PlayerController player) =>
        {
            Debug.Log("Sound Trap Triggered");
            GameController.AudioManager.AudibleEffect(gameObject, player.transform.position, SoundRange);
            GameController.AudioManager.Play("GlassShatter", position: transform.position);
            Destroy(gameObject);
        };
    }

    private void LateUpdate()
    {
        if (_lastInteract)
        {
        } else
        {
            _interactionTimer = InteractionTime;
        }

        _lastInteract = false;
    }

}
