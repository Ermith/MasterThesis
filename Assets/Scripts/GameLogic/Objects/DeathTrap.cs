using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrap : SmartCollider, ILockObject, IInteractableObject
{
    public ILock Lock { get; set; }

    public bool CanInteract => true;

    public InteractionType InteractionType => InteractionType.Continuous;

    public float InteractionTime = 1f;
    private float _interactionTimer;
    private bool _lastInteract = false;

    public float Interact(PlayerController player)
    {
        Debug.Log(_interactionTimer);
        _interactionTimer -= Time.deltaTime;
        Debug.Log(_interactionTimer);
        if (_interactionTimer < 0)
            Destroy(gameObject);

        _lastInteract = true;

        return 1 - _interactionTimer / InteractionTime;
    }

    public string InteractionPrompt()
    {
        return "Disarm";
    }

    public void Unlock()
    {
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
        if (_lastInteract)
        {
        } else
        {
            _interactionTimer = InteractionTime;
        }

        _lastInteract = false;
    }
}
