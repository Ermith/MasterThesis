using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactable object. Raises the number of objectives counter when spawned. Increases the number of objectives found when interacted with.
/// </summary>
public class SideObjectiveBox : MonoBehaviour, IInteractableObject
{
    public bool CanInteract => true;

    public InteractionType InteractionType => InteractionType.Continuous;

    [Tooltip("Time it takes to hold interaction button to finish interaction.")]
    public float InteractionTime = 1f;
    private float _interactionTimer;
    private bool _lastInteract = false;

    /// <summary>
    /// Continous interaction. Increases the number of objectives found.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>Percentage [0-1] of completness of this interaction.</returns>
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
        if (!_lastInteract)
        {
            _interactionTimer = InteractionTime;
        }

        _lastInteract = false;
    }
}
