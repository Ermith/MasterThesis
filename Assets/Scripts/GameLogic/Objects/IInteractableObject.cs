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

interface IInteractableObject
{
    bool CanInteract { get; }
    InteractionType InteractionType { get; }
    float Interact(PlayerController player);
    string InteractionPrompt();
}
