using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface IUsableObject
{
    bool IsUsable { get; }
    void Use(PlayerController player);
    string UsePrompt();
}
