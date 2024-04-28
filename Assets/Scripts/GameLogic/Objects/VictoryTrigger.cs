using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wins the game and restarts the level if the player moves through this object.
/// </summary>
public class VictoryTrigger : SmartCollider
{
    // Start is called before the first frame update
    void Start()
    {
        _triggerResponse += (PlayerController player) =>
        {
            var audio = GameController.AudioManager.Play("Victory");
            GameController.ExecuteAfter(GameController.Restart, audio.clip.length);
        };
    }
}
