using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum InteractionType
{
    Single,
    Continuous
}

/// <summary>
/// Gives ability for the object to be interacted with by holding Interact button. (F)
/// </summary>
interface IInteractableObject
{
    bool CanInteract { get; }
    /// <summary>
    /// Single means instant interaction. Continous means the player has to hold the button.
    /// </summary>
    InteractionType InteractionType { get; }
    /// <summary>
    /// Performs the interaction.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>Percentage [0-1] of completness of the interaction.</returns>
    float Interact(PlayerController player);

    /// <summary>
    ///
    /// </summary>
    /// <returns>What should be shown on the screen that the interaction does.</returns>
    string InteractionPrompt();
}
