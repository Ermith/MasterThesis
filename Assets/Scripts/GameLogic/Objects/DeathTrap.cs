using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kills the player if he moves through this object. Can be disabled by interaction.
/// Interaction time reduced to 0 if the player has <see cref="TrapDisarmingKitKey"/>. Consumes it on use.
/// </summary>
public class DeathTrap : SmartCollider, ILockObject, IInteractableObject
{
    [HideInInspector] public ILock Lock { get; set; }

    [HideInInspector] public bool CanInteract => true;

    [HideInInspector] public InteractionType InteractionType => InteractionType.Continuous;

    [Tooltip("Time it takes to interact if Trap Disarming not in inventory.")]
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

    /// <summary>
    /// Disables the game object.
    /// </summary>
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
            Debug.Log("Death Trap Triggered");
            player.Die();
        };
    }

    private void LateUpdate()
    {
        if (!_lastInteract)
        {
            _interactionTimer = InteractionTime;
        }

        _lastInteract = false;
    }
}
