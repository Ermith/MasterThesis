using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface IInteractableObject
{
    bool CanInteract { get; }
    void Interact(Player player);
    string InteractionPrompt();
}
