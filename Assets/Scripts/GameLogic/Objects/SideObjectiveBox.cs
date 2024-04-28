using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideObjectiveBox : MonoBehaviour, IInteractableObject
{
    public bool CanInteract => true;

    public InteractionType InteractionType => InteractionType.Continuous;

    public float InteractionTime = 1f;
    private float _interactionTimer;
    private bool _lastInteract = false;

    public float Interact(PlayerController player)
    {
        if (_interactionTimer >= InteractionTime)
            GameController.AudioManager.Play("Click");

        _interactionTimer -= Time.deltaTime;

        if (_interactionTimer <= 0)
        {
            GameController.ObjectivesFound++;
            GameController.AudioManager.Play("Cut", position:transform.position);
            Destroy(gameObject);
        }

        _lastInteract = true;

        return 1 - _interactionTimer / InteractionTime;
    }

    public string InteractionPrompt()
    {
        return "Complete Side Objective";
    }

    // Start is called before the first frame update
    void Start()
    {
        GameController.Objectives++;
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
